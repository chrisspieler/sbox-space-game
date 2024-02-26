using Sandbox;

public sealed class Jettison : Component
{
	[Property] public Rigidbody SelfRigidbody { get; set; }
	[Property] public GameObject Prefab { get; set; }
	[Property] public GameObject EjectionSource { get; set; }
	[Property] public float EjectionSpeed { get; set; } = 30f;
	[Property] public float EjectionSpin { get; set; } = 5f;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "flashlight" ) )
		{
			var jettisoned = Prefab.Clone( EjectionSource.Transform.World.WithScale( 1f ) );
			var jettisonedRb = jettisoned.Components.Get<Rigidbody>();
			var launchVelocity = EjectionSpeed * EjectionSource.Transform.Rotation.Forward;
			if ( SelfRigidbody is not null )
			{
				SelfRigidbody.Velocity -= launchVelocity * 0.3f;
				launchVelocity += SelfRigidbody.Velocity;
			}
			if ( jettisonedRb is not null )
			{
				jettisonedRb.Velocity = launchVelocity;
				jettisonedRb.AngularVelocity = EjectionSpin * Vector3.Random;
			}
		}
	}
}
