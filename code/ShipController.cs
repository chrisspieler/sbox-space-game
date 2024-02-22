using Sandbox;

public sealed partial class ShipController : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Acceleration { get; set; } = 200f;
	[Property] public float TurnSpeed { get; set; } = 1.5f;
	[Property, Category("Debug")] public Vector3 MainThrusterForce => _mainThrusterForce;
	private Vector3 _mainThrusterForce;
	[Property, Category("Debug")] public Vector3 RetrorocketForce => _retrorocketForce;
	private Vector3 _retrorocketForce;
	[Property, Category("Debug")] public Vector3 LastInputDir => _lastInputDir;
	private Vector3 _lastInputDir { get; set; }
	[Property, Category( "Equipment" )]
	public GrappleBeam Grapple { get; set; }
	[Property, Category( "Equipment" )]
	public Stabilizer Stabilizer { get; set; }

	public Rotation TargetRotation { get; private set; }

	protected override void OnStart()
	{
		_lastInputDir = PartsContainer.Transform.Rotation.Forward.WithZ( 0f );
	}

	protected override void OnUpdate()
	{
		var inputDir = Input.AnalogMove;
		if ( !inputDir.IsNearZeroLength )
		{
			_lastInputDir = GetLastKeyboardDirection();
		}
		UpdateThrusters( inputDir );
		var fromRot = PartsContainer.Transform.Rotation;
		TargetRotation = GetTargetRotation();
		var rotationSpeed = GetRotationSpeed();
		PartsContainer.Transform.Rotation = Rotation.Lerp( fromRot, TargetRotation, rotationSpeed * Time.Delta );
		UpdateDebugInfo();
	}

	private void UpdateThrusters( Vector3 inputDir )
	{
		_mainThrusterForce = PartsContainer.Transform.Rotation.Forward * GetMainThrusterForce();
		_retrorocketForce = Stabilizer?.GetStabilizerForce() ?? Vector3.Zero;
		Rigidbody.Velocity += _mainThrusterForce;
		Rigidbody.Velocity += _retrorocketForce;
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

	private float GetMainThrusterForce()
	{
		var acceleration = Acceleration * (Input.Down( "thrust" ) ? 1f : 0f);
		return acceleration * Time.Delta;
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
