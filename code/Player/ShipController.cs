using Sandbox;
using Sandbox.Utility;
using System.Linq;

public sealed partial class ShipController : Component
{
	[ConVar( "ship_spawn_invincibility_time" )]
	public static float SpawnInvincibilitySeconds { get; set; } = 1f;
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public float TurnSpeed { get; set; } = 0.5f;
	[Property] public Vector3 FacingDirection { get; set; }
	[Property] public bool IsInvincible 
	{
		get => _isInvincible;
		set
		{
			_isInvincible = value;
			if ( !Game.IsPlaying )
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

	public static ShipController Instance { get; private set; }
	public static ShipController GetCurrent()
	{
		return Instance;
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
		{
			Instance = null;
		}
	}

	protected override void OnStart()
	{
		Instance = this;
		FacingDirection = PartsContainer.Transform.Rotation.Forward.WithZ( 0f );
		ResetUI();
		ResetCamera();
		SetFogFollowTarget();
		FindEquipmentInChildren();
		FindWeaponsInChildren();
		ApplyAllUpgrades();
		AddTemporaryInvincibility( SpawnInvincibilitySeconds );
		GameObject.BreakFromPrefab();
	}

	public void AddTemporaryInvincibility( float duration )
	{
		IsInvincible = true;
		_ = Task.DelaySeconds( duration ).ContinueWith( _ => IsInvincible = GodMode );
	}

	private void SetFogFollowTarget()
	{
		var fogGo = Scene.GetAllComponents<VolumetricFogVolume>().FirstOrDefault();
		if ( fogGo is null )
			return;
		var follower = fogGo.Components.Get<Follower>();
		follower.Target = GameObject;
	}

	private void ResetCamera()
	{
		var cameraGo = Scene.Camera?.GameObject;
		if ( cameraGo is null )
			return;

		var camera = cameraGo.Components.Get<ShipCamera>( true );
		if ( camera is null )
			return;

		camera.Target = GameObject;
		camera.Transform.Position = Transform.Position + new Vector3( -500f, 0, 200f );
		camera.Transform.Rotation = Rotation.FromPitch( 30f );
		camera.ResetBaseRotation();
		camera.Enabled = true;
	}

	private void ResetUI()
	{
		ScreenManager.SetHudEnabled( true );
		ScreenManager.SetCursorEnabled( true );
		ScreenManager.UpdateShip( this );
		ScreenManager.SetBeaconVisibility( true );
	}

	protected override void OnUpdate()
	{
		if ( Scene.TimeScale == 0f )
			return;

		var inputDir = Input.AnalogMove;
		if ( !inputDir.IsNearZeroLength )
		{
			FacingDirection = GetLastKeyboardDirection();
		}
		UpdateThrusters();
		TargetRotation = GetTargetRotation();
		Rigidbody.PhysicsBody.SmoothRotate( TargetRotation, 1f / TurnSpeed, Time.Delta );
		PartsContainer.Transform.Rotation = Rigidbody.PhysicsBody.Rotation;
		UpdateDebugInfo();
	}

	private void UpdateThrusters()
	{
		MainThrusters.ShouldFire = ShouldFireMainThrusters();
		_mainThrusterForce = MainThrusters.ShouldFire
			? MainThrusters.GetForce()
			: Vector3.Zero;

		Retrorockets.ShouldFire = ShouldFireRetrorockets();
		_retrorocketForce = Stabilizer?.GetStabilizerForce() ?? Vector3.Zero;
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
}
