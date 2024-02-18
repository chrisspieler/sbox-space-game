using System;
using Sandbox;

public sealed class ShipController : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Acceleration { get; set; } = 200f;
	[Property] public float RetrorocketForceScale { get; set; } = 150f;
	[Property] public Vector3 MainThrusterForce => _mainThrusterForce;
	private Vector3 _mainThrusterForce;
	[Property] public Vector3 RetrorocketForce => _retrorocketForce;
	private Vector3 _retrorocketForce;
	public Rotation TargetRotation { get; private set; }

	[Property, Category( "Equipment" )]
	public GrappleBeam Grapple { get; set; }

	[Property] public Vector3 LastInputDir => _lastInputDir;
	private Vector3 _lastInputDir { get; set; } = Vector3.Forward;
	[Property] public float TurnSpeed { get; set; } = 1.5f;

	protected override void OnUpdate()
	{
		Transform.Rotation = Transform.Rotation.Angles().WithRoll( 0f );
		Rigidbody.AngularVelocity = Vector3.Zero;
		var inputDir = Input.AnalogMove;
		if ( !inputDir.IsNearZeroLength )
		{
			_lastInputDir = GetLastKeyboardDirection();
		}
		var velocity = Rigidbody.Velocity;
		_mainThrusterForce = PartsContainer.Transform.Rotation.Forward * GetMainThrusterForce( inputDir );
		velocity += _mainThrusterForce;
		_retrorocketForce = GetRetrorocketForce( inputDir, velocity );
		velocity += _retrorocketForce;
		Rigidbody.Velocity = velocity;
		var fromRot = PartsContainer.Transform.Rotation;
		TargetRotation = GetTargetRotation();
		var rotationSpeed = GetRotationSpeed();
		PartsContainer.Transform.Rotation = Rotation.Lerp( fromRot, TargetRotation, rotationSpeed * Time.Delta );
	}

	[ConVar("input_diagonal_key_grace")] 
	public static float KeyInputGraceWindow { get; set; } = 0.2f;
	private TimeSince _lastForward = 100f;
	private TimeSince _lastBackward = 100f;
	private TimeSince _lastLeft = 100f;
	private TimeSince _lastRight = 100f;

	private Vector3 GetLastKeyboardDirection()
	{
		if ( Input.Down( "forward" ) )
			_lastForward = 0f;
		if ( Input.Down( "backward" ) )
			_lastBackward = 0f;
		if ( Input.Down( "left" ) )
			_lastLeft = 0f;
		if ( Input.Down( "right" ) )
			_lastRight = 0f;

		var input = Vector3.Zero;
		if ( _lastForward < KeyInputGraceWindow )
			input += Vector3.Forward;
		if ( _lastBackward < KeyInputGraceWindow )
			input += Vector3.Backward;
		if ( _lastLeft < KeyInputGraceWindow )
			input += Vector3.Left;
		if ( _lastRight < KeyInputGraceWindow )
			input += Vector3.Right;

		return input.Normal;
	}

	private float GetMainThrusterForce( Vector3 inputDir )
	{
		var acceleration = Acceleration * (Input.Down( "thrust" ) ? 1f : 0f);
		return acceleration * Time.Delta;
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

	public bool IsGrappling => Grapple.IsValid() && Grapple.IsSlack;

	private Rotation GetTargetRotation()
	{
		return Rotation.LookAt( _lastInputDir, Vector3.Up );
	}

	private float GetRotationSpeed()
	{
		return (!Grapple.IsValid() || Grapple.IsSlack)
			? TurnSpeed
			: TurnSpeed * 3f;
	}
}
