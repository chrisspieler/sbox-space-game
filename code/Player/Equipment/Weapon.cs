using Sandbox;

public sealed class Weapon : Component
{
	[Property] public GameObject Body { get; set; }
	// Laser specific stuff, to be moved somewhere else.
	[Property] public GameObject TraceOrigin { get; set; }
	[Property] public GameObject LaserEffectPrefab { get; set; }
	[Property] public GameObject LaserAblationPrefab { get; set; }
	[Property] public float TickInterval { get; set; } = 0.2f;
	[Property] public float TickDamage { get; set; } = 2.5f;


	private GameObject _currentTarget;
	private LaserBeam _currentLaserEffect;
	private GameObject _laserHitTarget;
	private TimeUntil _untilNextDamageTick;

	protected override void OnUpdate()
	{
		UpdateBodyRotation();
		UpdateAttack();
	}

	private void UpdateAttack()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		var distance = TraceOrigin.Transform.Position.Distance( mousePos );
		// If we aren't attacking, don't show a laser, and don't damage anything.
		// Also, don't fire a beam through your own ship unless you very clearly mean to do so.
		if ( !Input.Down( "fire" ) || ( distance > 150f && IsAimingAtSelf( mousePos ) ) )
		{
			DestroyLaserEffect();
			EndDamage();
			return;
		}
		var tr = RunAimTrace( mousePos );
		_currentLaserEffect ??= CreateLaserEffect();
		UpdateLaserEffect( tr.EndPosition, tr.Hit, tr.Normal );
		var target = tr.GameObject;
		if ( target is null )
		{
			EndDamage();
			return;
		}

		if ( !target.Components.TryGet<IDamageable>( out var damageable, FindMode.EnabledInSelfAndDescendants ) )
			return;

		if ( _currentTarget != target )
		{
			StartDamage( target );
		}

		UpdateDamage( _currentTarget, damageable );
	}

	private bool IsAimingAtSelf( Vector3 endPosition )
	{
		var startPos = TraceOrigin.Transform.Position.WithZ( 0f );
		var endPos = endPosition;
		return Scene.Trace
			.Ray( startPos, endPos )
			.WithTag( "player" )
			.Run()
			.Hit;
	}

	private SceneTraceResult RunAimTrace( Vector3 endPosition )
	{
		var startPos = TraceOrigin.Transform.Position.WithZ( 0f );
		var ray = new Ray( startPos, startPos.Direction( endPosition ) );
		return Scene.Trace
			.Ray( ray, 2000f )
			.WithoutTags( "pickup" )
			.Run();
	}

	private LaserBeam CreateLaserEffect()
	{
		var laserGo = LaserEffectPrefab.Clone();
		laserGo.Parent = TraceOrigin;
		laserGo.Transform.Position = TraceOrigin.Transform.Position;
		laserGo.Transform.Rotation = TraceOrigin.Transform.Rotation;
		var laserBeam = laserGo.Components.Get<LaserBeam>();
		laserBeam.Tint = Color.Red;
		return laserBeam;
	}

	private void UpdateLaserEffect( Vector3 endPosition, bool hit, Vector3 normal )
	{
		_laserHitTarget ??= LaserAblationPrefab.Clone();
		_currentLaserEffect.Target ??= _laserHitTarget;
		_laserHitTarget.Enabled = hit;
		_laserHitTarget.Transform.Position = endPosition;
		_laserHitTarget.Transform.Rotation = Rotation.LookAt( normal );
	}

	private void DestroyLaserEffect()
	{
		_currentLaserEffect?.GameObject?.Destroy();
		_currentLaserEffect = null;
		_laserHitTarget?.Destroy();
		_laserHitTarget = null;
	}
	

	private void StartDamage( GameObject targetGo )
	{
		_currentTarget = targetGo;
		_untilNextDamageTick = TickInterval;
	}

	private void EndDamage()
	{
		_currentTarget = null;
	}

	private void UpdateDamage( GameObject targetGo, IDamageable damageable )
	{
		if ( !_untilNextDamageTick )
			return;

		_untilNextDamageTick = TickInterval;
		var damageInfo = new DamageInfo()
		{
			Attacker = ShipController.GetCurrent().GameObject,
			Damage = TickDamage,
			Position = targetGo.Transform.Position,
			Weapon = GameObject
		};
		damageable.OnDamage( damageInfo );
	}

	private void UpdateBodyRotation()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		var direction = (mousePos - Body.Transform.Position).Normal;
		var yaw = Rotation.LookAt( direction ).Yaw();
		Body.Transform.Rotation = ((Angles)Body.Transform.Rotation).WithYaw( yaw );
	}
}
