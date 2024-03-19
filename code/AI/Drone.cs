using Sandbox;

public sealed class Drone : Component
{
	public enum DroneState
	{
		Idle,
		Busy,
		Hostile
	}

	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject NavTargetGameObject
	{
		get => _navTargetGameObject;
		set
		{
			_navTargetGameObject = value;
			NavTarget = new Target( value );
		}
	}
	private GameObject _navTargetGameObject;
	[Property] public DroneState State 
	{
		get => _state;
		set
		{
			if ( value != _state )
			{
				// TODO: Handle state transition.
			}
			_state = value;
		}
	}
	private DroneState _state;
	[Property] public Health Hull { get; set; }

	public Target? NavTarget { get; set; }

	private Vector3 _navDirection { get; set; }

	protected override void OnStart()
	{
		if ( Hull is not null )
		{
			Hull.OnDamaged += OnDamaged;
		}
		GameObject.BreakFromPrefab();
	}

	protected override void OnValidate()
	{
		Rigidbody ??= Components.Get<Rigidbody>();
		Hull ??= Components.Get<Health>();
	}

	protected override void OnUpdate()
	{
		switch ( State )
		{
			case DroneState.Idle:
				UpdateIdleState();
				break;
			case DroneState.Busy:
				UpdateBusyState(); 
				break;
			case DroneState.Hostile:
				UpdateHostileState(); 
				break;
		}
		UpdateNavigation();
	}

	protected override void OnFixedUpdate()
	{
		UpdateThruster( _navDirection, 300f, 0.1f );
	}

	private void UpdateIdleState()
	{
		if ( NavTarget is not null )
			return;

		var randomPoint = Scene.NavMesh.GetRandomPoint( Transform.Position, 5000f );
		if ( randomPoint.HasValue )
		{
			NavTarget = Target.FromRelativePosition( randomPoint.Value );
		}
	}

	private void UpdateBusyState()
	{

	}

	private void UpdateHostileState()
	{
		var player = ShipController.GetCurrent();
		// When the player dies, go back to business as usual.
		if ( player is null )
		{
			State = DroneState.Idle;
			return;
		}

		if ( NavTarget is null || NavTarget.Value.GameObject != player.GameObject )
		{
			NavTarget = new Target( player.GameObject );
		}
	}

	private void OnDamaged( DamageInfo damage )
	{
		if ( damage.Attacker.Tags.Has( "player" ) )
		{
			State = DroneState.Hostile;
		}
	}

	private void UpdateNavigation()
	{
		if ( HasReachedTarget( 100f ) )
			NavTarget = null;

		_navDirection = GetDirectionToTarget();
	}

	private bool HasReachedTarget( float distance )
	{
		if ( NavTarget is null )
			return false;

		return Transform.Position.Distance( NavTarget.Value.RelativePosition ) < distance;
	}

	private Vector3 GetDirectionToTarget()
	{
		if ( NavTarget is null )
			return Vector3.Zero;

		return Transform.Position.Direction( NavTarget.Value.RelativePosition );
	}

	private void UpdateThruster( Vector3 direction, float maxSpeed, float acceleration )
	{
		var targetVelocity = direction * maxSpeed;
		Rigidbody.Velocity = Rigidbody.Velocity.WithAcceleration( targetVelocity, acceleration );
	}
}
