using Sandbox;

public sealed class Jettison : Component
{
	[Property] public GameObject Prefab { get; set; }
	[Property] public GameObject EjectionSource { get; set; }
	[Property] public float EjectionSpeed { get; set; } = 30f;
	[Property] public float EjectionSpin { get; set; } = 5f;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "flashlight" ) )
		{
			var jettisoned = SceneUtility.Instantiate( Prefab, EjectionSource.Transform.World );
			var jettisonedRb = jettisoned.Components.Get<Rigidbody>();
			var launchVelocity = EjectionSpeed * EjectionSource.Transform.Rotation.Forward;
			if ( jettisonedRb is not null )
			{
				jettisonedRb.Velocity = launchVelocity;
				jettisonedRb.AngularVelocity = EjectionSpin * Vector3.Random;
			}
			var shipRb = Components.Get<Rigidbody>();
			if ( shipRb is not null )
			{
				shipRb.Velocity -= launchVelocity * 0.3f;
			}
		}
	}
}
