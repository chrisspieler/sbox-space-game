using Sandbox;
using System.Linq;

public sealed class DeathCam : Component, IBasisSource
{
	[Property] public GameObject Target { get; set; }
	[Property] public Vector3 Offset { get; set; } = Vector3.Backward * 300f;
	[Property] public Curve PositionLerpSpeed { get; set; }
	[Property] public Curve RotationLerpSpeed { get; set; }

	protected override void OnEnabled()
	{
		Scene.Camera?.Components?.Get<ShipCamera>()?.SetEnabled( false );
	}

	public Transform GetBaseTransform( Transform lastTransform )
	{
		if ( !Target.IsValid() )
			return lastTransform;

		var targetPosition = Target.Transform.Position + Offset;
		var distanceToTargetPosition = lastTransform.Position.Distance( targetPosition );
		var targetRotation = Rotation.LookAt( (Target.Transform.Position - lastTransform.Position).Normal );
		var posLerpSpeed = PositionLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var position = lastTransform.Position.LerpTo( targetPosition, Time.Delta * posLerpSpeed );
		var rotLerpSpeed = RotationLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var rotation = Rotation.Slerp( lastTransform.Rotation, targetRotation, Time.Delta * rotLerpSpeed );
		return new Transform( position, rotation );
	}

	public static void Begin( GameObject target )
	{
		var deathCam = Game.ActiveScene?.Camera?.Components?.Get<DeathCam>( true );
		if ( target is null || deathCam is null )
			return;

		deathCam.Enabled = true;
		deathCam.Target = target;
	}

	public static void End()
	{
		Game.ActiveScene?.Camera?.Components?.Get<DeathCam>()?.SetEnabled( false );
	}
}
