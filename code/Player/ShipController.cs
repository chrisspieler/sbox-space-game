using Sandbox;
using Sandbox.Utility;

public sealed partial class ShipController : Component
{
	[ConVar( "ship_spawn_invincibility_time" )]
	public static float SpawnInvincibilitySeconds { get; set; } = 1f;
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float Acceleration { get; set; } = 200f;
	[Property] public float TurnSpeed { get; set; } = 1.5f;
	[Property] public Vector3 FacingDirection { get; set; }
	[Property] public bool IsInvincible 
	{
		get => _isInvincible;
		set
		{
			_isInvincible = value;
			if ( !GameManager.IsPlaying )
				return;

			if ( _hull is not null )
			{
				_hull.IsInvincible = value;
			}

			if ( !_isInvincible )
				return;
			
			var tintEffect = new TintEffect()
			{
				BlendMode = ColorBlendMode.Normal,
				Tint = Color.White,
				EasingFunction = Easing.GetFunction( "ease-in" ),
				UntilFadeEnd = SpawnInvincibilitySeconds
			};
			GameObject.ColorFlash( tintEffect );
		}
	}
	private bool _isInvincible;
	[Property, Category("Debug")] public Vector3 MainThrusterForce => _mainThrusterForce;
	private Vector3 _mainThrusterForce;
	[Property, Category("Debug")] public Vector3 RetrorocketForce => _retrorocketForce;
	private Vector3 _retrorocketForce;

	public Rotation TargetRotation { get; private set; }

	protected override void OnStart()
	{
		FacingDirection = PartsContainer.Transform.Rotation.Forward.WithZ( 0f );
		ScreenManager.SetHudEnabled( true );
		ScreenManager.SetCursorEnabled( true );
		ScreenManager.UpdateShip( this );
		ScreenManager.SetBeaconVisibility( true );
		FindEquipmentInChildren();
		GameObject.BreakFromPrefab();
		IsInvincible = true;
		_ = Task.DelaySeconds( SpawnInvincibilitySeconds ).ContinueWith( _ => IsInvincible = GodMode );
	}

	protected override void OnUpdate()
	{
		var inputDir = Input.AnalogMove;
		if ( !inputDir.IsNearZeroLength )
		{
			FacingDirection = GetLastKeyboardDirection();
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
		_retrorocketForce = Stabilizer?.GetStabilizerForce() ?? Vector3.Zero;
		_mainThrusterForce = Vector3.Zero;
		foreach( var thruster in Thrusters )
		{
			thruster.ShouldFire = thruster.Retrorocket
				? ShouldFireRetrorockets()
				: ShouldFireMainThrusters();

			// Store main thruster force for debug visualization.
			if ( thruster.ShouldFire && !thruster.Retrorocket )
			{
				_mainThrusterForce += thruster.GetForce();
			}
		}
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

	private bool ShouldFireMainThrusters()
	{
		return Input.Down( "thrust" );
	}

	private bool ShouldFireRetrorockets()
	{
		return !_retrorocketForce.IsNearZeroLength;
	}

	public bool IsGrappling => Grapple.IsValid() && Grapple.IsSlack;

	public bool IsAlive => Hull.IsValid() && Hull.CurrentHealth > 0f;

	private Rotation GetTargetRotation()
	{
		return Rotation.LookAt( FacingDirection, Vector3.Up );
	}

	private float GetRotationSpeed()
	{
		return (!Grapple.IsValid() || Grapple.IsSlack)
			? TurnSpeed
			: TurnSpeed * 3f;
	}
}
