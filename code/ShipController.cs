using System;
using Sandbox;

public sealed class ShipController : Component
{
	[Property] public PhysicsComponent Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Speed { get; set; } = 100f;
	[Property] public float RetrorocketForce { get; set; } = 150f;

	protected override void OnUpdate()
	{
		var inputDir = Input.AnalogMove;
		var velocity = Rigidbody.Velocity;
		velocity += inputDir * Speed * Time.Delta;
		if ( inputDir.x == 0 || MathF.Sign( inputDir.x ) != MathF.Sign( velocity.x ) )
		{
			velocity.x = velocity.x.Approach( 0f, RetrorocketForce * Time.Delta );
		}
		if ( inputDir.y == 0 || MathF.Sign( inputDir.y ) != MathF.Sign( velocity.y ) )
		{
			velocity.y = velocity.y.Approach( 0f, RetrorocketForce * Time.Delta );
		}
		Rigidbody.Velocity = velocity;
		if ( inputDir.LengthSquared < 0.01f )
		{
			return;
		}
		var fromRot = PartsContainer.Transform.Rotation;
		var toRot = Rotation.LookAt( inputDir, Vector3.Up );
		PartsContainer.Transform.Rotation = Rotation.Lerp( fromRot, toRot, 3f * Time.Delta );
	}
}
