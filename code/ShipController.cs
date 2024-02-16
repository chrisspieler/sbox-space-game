using System;
using Sandbox;

public sealed class ShipController : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Acceleration { get; set; } = 200f;
	[Property] public float RetrorocketForceScale { get; set; } = 150f;
	public Vector3 MainThrusterForce { get; private set; }
	public Vector3 RetrorocketForce { get; private set; }
	public Rotation TargetRotation { get; private set; }

	[Property] public Vector3 LastInputDir => _lastInputDir;
	private Vector3 _lastInputDir { get; set; } = Vector3.Forward;

	protected override void OnUpdate()
	{
		Transform.Rotation = Transform.Rotation.Angles().WithRoll( 0f );
		Rigidbody.AngularVelocity = Vector3.Zero;
		var inputDir = Input.AnalogMove;
		if ( !inputDir.IsNearZeroLength )
		{
			_lastInputDir = inputDir;
		}
		var velocity = Rigidbody.Velocity;
		MainThrusterForce = GetMainThrusterForce( inputDir );
		velocity += MainThrusterForce;
		RetrorocketForce = GetRetrorocketForce( inputDir, velocity );
		velocity += RetrorocketForce;
		Rigidbody.Velocity = velocity;
		var fromRot = PartsContainer.Transform.Rotation;
		TargetRotation = Rotation.LookAt( _lastInputDir, Vector3.Up );
		PartsContainer.Transform.Rotation = Rotation.Lerp( fromRot, TargetRotation, 1.5f * Time.Delta );
	}

	private Vector3 GetMainThrusterForce( Vector3 inputDir )
	{
		var acceleration = Acceleration * (Input.Down( "jump" ) ? 1f : 0f);
		var direction = inputDir.IsNearZeroLength ? _lastInputDir : inputDir;
		return direction * acceleration * Time.Delta;
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
