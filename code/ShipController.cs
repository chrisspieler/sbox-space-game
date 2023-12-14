using System;
using Sandbox;

public sealed class ShipController : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Speed { get; set; } = 100f;
	[Property] public float RetrorocketForceScale { get; set; } = 150f;
	public Vector3 MainThrusterForce { get; private set; }
	public Vector3 RetrorocketForce { get; private set; }

	protected override void OnUpdate()
	{
		Transform.Rotation = Transform.Rotation.Angles().WithRoll( 0f );
		Rigidbody.AngularVelocity = Vector3.Zero;
		var inputDir = Input.AnalogMove;
		MainThrusterForce = inputDir * Speed * Time.Delta;
		var velocity = Rigidbody.Velocity;
		velocity += MainThrusterForce;
		RetrorocketForce = GetRetrorocketForce( inputDir, velocity );
		velocity += RetrorocketForce;
		Rigidbody.Velocity = velocity;
		if ( inputDir.LengthSquared < 0.01f )
		{
			return;
		}
		var fromRot = PartsContainer.Transform.Rotation;
		var toRot = Rotation.LookAt( inputDir, Vector3.Up );
		PartsContainer.Transform.Rotation = Rotation.Lerp( fromRot, toRot, 3f * Time.Delta );
	}

	private Vector3 GetRetrorocketForce( Vector3 inputDir, Vector3 velocity )
	{
		var force = Vector3.Zero;
		if ( velocity.LengthSquared < 0.1f )
			return -velocity * Time.Delta;

		if ( inputDir.x == 0 || MathF.Sign( inputDir.x ) != MathF.Sign( velocity.x ) )
		{
			var xForce = RetrorocketForceScale * Time.Delta;
			// Retrorocket force is applied in the opposite direction of velocity.
			xForce *= -MathF.Sign( velocity.x );
			// If the amount of xForce would flip the sign of x velocity...
			if ( MathF.Abs( velocity.x ) < MathF.Abs( xForce ) )
			{
				// ...then just apply enough force to zero out x velocity.
				xForce = MathF.Abs( velocity.x );
			}
			force += Vector3.Zero.WithX( xForce );
		}
		if ( inputDir.y == 0 || MathF.Sign( inputDir.y ) != MathF.Sign( velocity.y ) )
		{
			var yForce = RetrorocketForceScale * Time.Delta;
			yForce *= -MathF.Sign( velocity.y );
			if ( MathF.Abs( velocity.y ) < MathF.Abs( yForce ) )
			{
				yForce = MathF.Abs( velocity.y );
			}
			force += Vector3.Zero.WithY( yForce );
		}
		return force;
	}
}
