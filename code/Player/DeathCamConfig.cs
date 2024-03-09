using Sandbox;
using System.Linq;

public sealed class DeathCam : Component, IBasisSource
{
	[Property] public DeathCamTarget Target { get; set; }
	[Property] public float Distance { get; set; } = 40f;
	[Property] public Curve PositionLerpSpeed { get; set; }
	[Property] public Curve RotationLerpSpeed { get; set; }
	[Property] public float LookbackDistance { get; set; } = 200f;

	protected override void OnEnabled()
	{
		Scene.Camera?.Components?.Get<ShipCamera>()?.SetEnabled( false );
	}

	public Transform GetBaseTransform( Transform lastTransform )
	{
		if ( Target is null )
			return lastTransform;

		var targetPosition = Target.Transform.Position + Target.Transform.World.Forward * Distance;
		var distanceToTargetPosition = lastTransform.Position.Distance( targetPosition );
		var targetRotation = distanceToTargetPosition > LookbackDistance
			? Rotation.LookAt( (targetPosition - lastTransform.Position).Normal )
			: Rotation.LookAt( Target.Transform.Rotation.Backward );
		var posLerpSpeed = PositionLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var position = lastTransform.Position.LerpTo( targetPosition, Time.Delta * posLerpSpeed );
		var rotLerpSpeed = RotationLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var rotation = Rotation.Slerp( lastTransform.Rotation, targetRotation, Time.Delta * rotLerpSpeed );
		return new Transform( position, rotation );
	}

	public static void Begin( DeathCamTarget target )
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
