using Sandbox;
using System.Linq;

public sealed class DeathCamConfig : Component
{
	[Property] public float Distance { get; set; } = 40f;
	[Property] public float FieldOfView { get; set; } = 90f;
	[Property] public Curve PositionLerpSpeed { get; set; }
	[Property] public Curve RotationLerpSpeed { get; set; }

	protected override void OnEnabled()
	{
		var shipCam = Scene.GetAllComponents<ShipCamera>().FirstOrDefault();
		if ( shipCam.IsValid() )
		{
			shipCam.Enabled = false;
		}
	}

	protected override void OnUpdate()
	{
		var cameraTx = Scene.Camera.Transform.World;
		var targetPosition = Transform.Position + Transform.World.Forward * Distance;
		var distanceToTargetPosition = cameraTx.Position.Distance( targetPosition );
		var targetRotation = distanceToTargetPosition > 200f
			? Rotation.LookAt( (targetPosition - cameraTx.Position).Normal )
			: Rotation.LookAt( Transform.Rotation.Backward );
		var posLerpSpeed = PositionLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var position = cameraTx.Position.LerpTo( targetPosition, Time.Delta * posLerpSpeed );
		var rotLerpSpeed = RotationLerpSpeed.Evaluate( MathX.LerpInverse( distanceToTargetPosition, 1000f, 0f ) );
		var rotation = Rotation.Slerp( cameraTx.Rotation, targetRotation, Time.Delta * rotLerpSpeed );
		Scene.Camera.Transform.World = new Transform( position, rotation );
		Scene.Camera.FieldOfView = FieldOfView;
	}
}
