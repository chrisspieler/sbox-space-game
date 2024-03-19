using Sandbox;

public sealed class Drone : Component
{
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

	public Target? NavTarget { get; set; }

	private Vector3 _navDirection { get; set; }

	protected override void OnUpdate()
	{
		UpdatePlan();
		UpdateNavigation();
	}

	protected override void OnFixedUpdate()
	{
		UpdateThruster( _navDirection, 300f, 0.1f );
	}

	private void UpdatePlan()
	{
		if ( NavTarget is not null )
			return;

		var player = ShipController.GetCurrent();
		if ( player is null )
			return;

		NavTarget = new Target( player.GameObject );
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
