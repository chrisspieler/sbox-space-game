using Sandbox;
using Sandbox.Utility;
using System;

public sealed class ShipHull : Component, Component.ICollisionListener
{
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public TagSet HitTags { get; set; }
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
	}

	public void OnCollisionStart( Collision other )
	{
		if ( !CanHit( other.Other.GameObject ) )
			return;

		other.Pongify( 0.2f );
		Hit( other.GetDamage() );
	}

	public void OnCollisionStop( CollisionStop other ) { }

	public void OnCollisionUpdate( Collision other ) { }

	private bool CanHit( GameObject other )
	{
		return HitTags is null
			|| HitTags.IsEmpty
			|| other.Tags.HasAny( HitTags );
	}

	public void Hit( float damage )
	{
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		var tinters = PartsContainer.Components.GetAll<TintCombiner>();
		foreach ( var tinter in tinters )
		{
			tinter.AddTint( Color.Red, 1f, ColorBlendMode.Normal, Easing.GetFunction( "ease-out" ) );
		}
	}
}
