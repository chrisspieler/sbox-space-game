using Sandbox;
using Sandbox.Utility;
using System;

public sealed class RagdollFlail : Component
{
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
	}

	protected override void OnFixedUpdate()
	{
		FlailBody( _leftUpperArm, 0f, 30f, 0 );
		FlailBody( _rightUpperArm, 0f, 30f, 1 );
		FlailBody( _leftLowerArm, -30f, 30f, 2 );
		FlailBody( _rightLowerArm, -30f, 30f, 3 );
		FlailBody( _leftShin, -90f, 0f, 8 );
		FlailBody( _rightShin, -90f, 0f, 9 );
	}

	private void FlailBody( PhysicsBody body, float min, float max, int offset )
	{
		if ( !body.IsValid() )
			return;

		var noise = Noise.Perlin( Time.Now * NoiseIntensity + offset * 100f + _individualSeed );
		var pitch = noise.Remap( 0f, 1f, min, max );
		var rotation = Rotation.FromPitch( pitch );
		rotation *= Rotation.FromYaw( pitch * 0.5f );
		rotation = body.Transform.RotationToLocal( rotation );
		body.SmoothRotate( rotation, 1f / RotateSpeed, Scene.FixedDelta );
	}
}
