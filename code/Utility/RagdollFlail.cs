using Sandbox;
using System;

public sealed class RagdollFlail : Component
{
	public static void LoadTestMap()
	{
		if ( Game.ActiveScene is null )
			return;

		Game.ActiveScene.Load( ResourceLibrary.Get<SceneFile>( "scenes/test_ragdoll.scene" ) );
	}

	[Property, RequireComponent] public ModelPhysics Ragdoll { get; set; }
	[Property, Range(1, 100, 1)] public float NoiseIntensity { get; set; } = 30f;
	[Property, Range( 1, 100, 1 )] public float RotateSpeed { get; set; } = 20f;

	private float _individualSeed;
	private PhysicsBody _leftUpperArm;
	private PhysicsBody _rightUpperArm;
	private PhysicsBody _leftLowerArm;
	private PhysicsBody _rightLowerArm;
	private PhysicsBody _leftShin;
	private PhysicsBody _rightShin;

	protected override void OnStart()
	{
		_individualSeed = Random.Shared.Next( 0, 2500 );
		_leftUpperArm = Ragdoll.PhysicsGroup.GetBody( "arm_upper_L" );
		_rightUpperArm = Ragdoll.PhysicsGroup.GetBody( "arm_upper_R" );
		_leftLowerArm = Ragdoll.PhysicsGroup.GetBody( "arm_lower_L" );
		_rightLowerArm = Ragdoll.PhysicsGroup.GetBody( "arm_lower_R" );
		_leftShin = Ragdoll.PhysicsGroup.GetBody( "leg_lower_L" );
		_rightShin = Ragdoll.PhysicsGroup.GetBody( "leg_lower_R" );
		foreach ( var body in Ragdoll.PhysicsGroup.Bodies )
		{
			// Apply angular damping to counteract the massive amounts of speen
			// caused by flailing beyond the limits of a joint. 
			body.AngularDamping = 5f;
		}
		// Set face_override to "surprise"
		Ragdoll.Renderer.SceneModel.SetAnimParameter( "face_override", 3 );
	}

	protected override void OnFixedUpdate()
	{
		FlailBody( _leftUpperArm, 0f, 20f, 0 );
		FlailBody( _rightUpperArm, 0f, 20f, 2 );
		FlailBody( _leftLowerArm, -40f, 40f, 3 );
		FlailBody( _rightLowerArm, -40f, 40f, 5 );
		FlailBody( _leftShin, -90f, -30f, 3 );
		FlailBody( _rightShin, -90f, -30f, 5 );
	}

	private void FlailBody( PhysicsBody body, float min, float max, int offset )
	{
		if ( !body.IsValid() )
			return;

		var pitch = MathF.Sin( Time.Now * NoiseIntensity + offset * 1.57f + _individualSeed )
			.Remap( -1f, 1f, min, max );
		var rotation = Rotation.FromPitch( pitch );
		// We'll add some yaw and roll intermittently to both add variety and also
		// provide a potential avenue of escape from "chicken arm hell".
		rotation *= Rotation.FromYaw( pitch * 0.5f * MathF.Sin( Time.Now * 3f + 5f ) );
		rotation *= Rotation.FromRoll( pitch * 0.5f * MathF.Sin( Time.Now * 6f + 10f ) );
		rotation = body.Transform.RotationToLocal( rotation );
		body.SmoothRotate( rotation, 1f / RotateSpeed, Scene.FixedDelta );
	}
}
