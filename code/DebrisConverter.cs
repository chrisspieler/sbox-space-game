using System;
using System.Linq;

using Sandbox;

public sealed class DebrisConverter : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }

	public void ReleaseDebris()
	{
		ReleaseDebris( GameObject );
	}

	private void ReleaseDebris( GameObject go )
	{
		var parts = go.Children
			.Where( c => c.Tags.Has( "break_debris" ) && !c.Tags.Has( "break_child" ) )
			.ToList();
		var breakChildren = go.Children
			.Where( c => c.Tags.Has( "break_child" ) )
			.ToList();
		foreach ( var child in breakChildren )
		{
			foreach ( var collider in child.Components.GetAll<Collider>( FindMode.EverythingInSelfAndDescendants ) )
			{
				collider.Enabled = true;
			}
		}
		foreach ( var debris in parts )
		{
			ReleaseDebris( debris );

			var oldTx = debris.Transform.World;
			debris.Name = $"(Debris) {debris.Name}";
			debris.Parent = null;
			debris.Transform.World = oldTx;
			// To prevent being ejected by the base shield.
			if ( Tags.Has( "player" ) )
			{
				debris.Tags.Add( "player" );
			}

			// To allow for the ship hitbox to be smaller than the visible model,
			// all colliders for ship parts should remain disabled until they are released as debris.
			if ( debris.Components.TryGet<Collider>( out var collider, FindMode.EverythingInSelf ) )
			{
				collider.Enabled = true;
			}

			if ( debris.Components.TryGet<IDestructionListener>( out var listener, FindMode.EverythingInSelf ) )
			{
				listener.OnMakeDebris();
			}

			if ( Rigidbody.IsValid() && debris.Components.TryGet<Rigidbody>( out var rb, FindMode.EverythingInSelf ) )
			{
				rb.Enabled = true;
				rb.Velocity = Rigidbody.Velocity;
				rb.Velocity += Vector3.Random.WithZ( 0f ) * 200f;
				// Not sure why this would ever be null, but I get NREs sometimes.
				if ( rb.PhysicsBody is not null )
				{
					// Make sure the mass of each piece of ship debris is low enough that the ship can push it.
					rb.PhysicsBody.Mass = Math.Min( rb.PhysicsBody.Mass, 100f );
				}
				rb.AngularVelocity += Vector3.Random * 3f;
			}
		}
	}
}
