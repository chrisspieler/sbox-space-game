using System.Collections.Generic;

using Sandbox;

public sealed class BlastSource : Component
{
	[ConVar( "pickup_knockback_scale" )]
	public static float PickupKnockbackScale { get; set; } = 0.05f;
	[Property] public float Force { get; set; } = 2000f;
	[Property] public float Radius { get; set; } = 800f;
	[Property] public bool BlastOnEnable { get; set; } = true;
	[Property] public bool DestroyAfterBlast { get; set; } = true;

	protected override void OnEnabled()
	{
		if ( BlastOnEnable )
		{
			DoBlast();
		}
	}

	public void DoBlast()
	{
		foreach( var nearby in GetNearby() )
		{
			PropelObject( nearby );
		}

		DoScreenShake();

		if ( DestroyAfterBlast )
		{
			Destroy();
		}
	}

	private IEnumerable<Rigidbody> GetNearby()
	{
		var nearby = Scene.FindInPhysics( new Sphere( Transform.Position, Radius ) );
		List<Rigidbody> hits = new();
		foreach ( var hit in nearby )
		{
			var rb = hit.Components.GetInAncestorsOrSelf<Rigidbody>();
			if ( rb.IsValid() && !hits.Contains( rb ) )
			{
				hits.Add( rb );
			}
		}
		return hits;
	}

	private void PropelObject( Rigidbody rb )
	{
		var distance = rb.Transform.Position.Distance( Transform.Position );
		var intensity = distance.LerpInverse( Radius, 0f );
		var direction = (rb.Transform.Position - Transform.Position).Normal;
		var impulse = Force * intensity * direction * GetForceScaleForObject( rb );
		rb.ApplyImpulse( impulse );
	}

	private static float GetForceScaleForObject( Rigidbody rb )
	{
		return rb.Tags.Has( "pickup" )
			? PickupKnockbackScale
			: 1f;
	}

	private void DoScreenShake()
	{
		var ship = ShipController.GetCurrent();
		var camera = ShipCamera.GetCurrent();
		if ( ship is null || camera is null )
			return;
		var distance = Transform.Position.Distance( ship.Transform.Position );
		camera.Trauma += distance.LerpInverse( 2000f, 0f ) * 1f;
	}
}
