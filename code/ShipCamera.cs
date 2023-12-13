using Sandbox;

public sealed class ShipCamera : Component
{
	[Property] public GameObject Target { get; set; }
	[Property, Range(30f, 90f, 1f)] public float LowPitch { get; set; } = 70f;
	[Property, Range(30f, 90f, 1f)] public float HighPitch { get; set; } = 88f;
	[Property] public Vector3 LowPosition { get; set; } = new Vector3( -150, 0f, 600f );
	[Property] public Vector3 HighPosition { get; set; } = new Vector3( -200, 0f, 1500f );
	[Property, Range( 0f, 1000f, 25f )] public float TargetVelocityLowThreshold { get; set; } = 200f;
	[Property, Range( 0f, 5000f, 25f )] public float TargetVelocityHighThreshold { get; set; } = 2500f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PitchLerpSpeed { get; set; } = 1f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PositionLerpSpeed { get; set; } = 1f;

	protected override void OnUpdate()
	{
		if ( Target?.Components?.TryGet<PhysicsComponent>( out var targetPhysics ) != true )
			return;

		var targetVelocity = targetPhysics.Velocity.Length;
		var lerpProgress = MathX.LerpInverse( targetVelocity, TargetVelocityLowThreshold, TargetVelocityHighThreshold );
		var targetPos = Vector3.Lerp( LowPosition, HighPosition, lerpProgress );
		Transform.LocalPosition = Transform.LocalPosition.LerpTo( targetPos, PositionLerpSpeed * Time.Delta );
		var targetRot = Rotation.Lerp( Rotation.FromPitch( LowPitch ), Rotation.FromPitch( HighPitch ), lerpProgress );
		Transform.LocalRotation = Rotation.Slerp( Transform.LocalRotation, targetRot, PitchLerpSpeed * Time.Delta );
	}
}
