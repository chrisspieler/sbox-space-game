using Sandbox;
using System;

public sealed class Weapon : Component, IDestructionListener
{
	[Property] public GameObject Body { get; set; }
	// Laser specific stuff, to be moved somewhere else.
	[Property] public GameObject TraceOrigin { get; set; }
	[Property] public GameObject LaserEffectPrefab { get; set; }
	[Property] public GameObject LaserAblationPrefab { get; set; }
	[Property] public GameObject ShieldHitPrefab { get; set; }
	[Property] public float LaserPower => MathX.LerpInverse( TickDamage, 0, MaxDamage );
	[Property] public float LaserDistance => MathX.Lerp( 250f, 3000f, LaserPower );
	[Property] public float TickInterval { get; set; } = 0.2f;
	[Property] public float TickDamage { get; set; } = 2.5f;
	[Property] public float MaxDamage { get; set; } = 20f;
	[Property] public float MinDamage { get; set; } = 2.5f;
	/// <summary>
	/// Offsets the damage tick progress by a fraction of the TickInterval, staggering
	/// damage ticks between weapons so they don't make sounds at the same time.
	/// </summary>
	[Property, Range( 0f, 1f )] public float DamagePhaseOffset { get; set; } = 0f;
	[Property] public bool DamageRampUp { get; set; } = true;
	[Property] public float DamageRampUpSpeed { get; set; } = 2.5f;
	[Property] public Curve DamageAblationScale { get; set; }
	[Property] public Gradient DamageColorScale { get; set; }
	[Property] public Curve DamagePitchScale { get; set; }
	[Property] public Color LaserTint { get; set; } = Color.Red;
	[Property] public SoundEvent LaserLoopSound { get; set; }

	private GameObject _currentTarget;
	private LaserBeam _currentLaserEffect;
	private GameObject _laserHitTarget;
	private SoundHandle _currentLaserSound;
	private TimeUntil _untilNextDamageTick;
	// We need a couple of timers to rate limit contributions to screen shake,
	// otherwise feathering/grazing a target with a laser will cause extreme shaking.
	private TimeUntil _untilScreenPunch = 0f;

	private static float _nextPhaseOffset = 0f;

	protected override void OnStart()
	{
		DamagePhaseOffset = _nextPhaseOffset;
		_nextPhaseOffset += 0.5f;
		_nextPhaseOffset %= 1f;
	}

	protected override void OnUpdate()
	{
		if ( ShipController.GetCurrent() is null || Scene.TimeScale == 0f )
			return;

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
			if ( DamageRampUp )
			{
				TickDamage = MinDamage;
			}
			DestroyLaserEffect();
			EndDamage();
			return;
		}

		if ( DamageRampUp )
		{
			TickDamage += Time.Delta * DamageRampUpSpeed;
			TickDamage = MathF.Min( TickDamage, MaxDamage );
		}

		var tr = RunAimTrace( mousePos );
		_currentLaserEffect ??= CreateLaserEffect();
		UpdateLaserEffect( tr );
		UpdateSound();

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

		UpdateScreenShake();
		UpdateDamage( tr, damageable );
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
			.Ray( ray, LaserDistance )
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
		return laserBeam;
	}

	private bool _wasHittingShield;

	private void UpdateLaserEffect( SceneTraceResult tr )
	{
		_currentLaserEffect.Tint = Color.Red.ToHsv().WithHue( Time.Now * 90f ).WithValue( LaserPower );
		var hitShield = tr.GameObject?.Tags?.Has( "shield" ) == true;
		if ( hitShield != _wasHittingShield && _laserHitTarget != null )
		{
			OrphanizeEffect( _laserHitTarget );
			_laserHitTarget = null;
		}
		_wasHittingShield = hitShield;
		_laserHitTarget ??= hitShield
			? ShieldHitPrefab.Clone()
			: LaserAblationPrefab.Clone();
		_currentLaserEffect.Target = _laserHitTarget;
		_laserHitTarget.ToggleParticleEmission( tr.Hit );
		_laserHitTarget.Transform.Position = tr.EndPosition;
		_laserHitTarget.Transform.Rotation = Rotation.LookAt( tr.Normal );
		// TODO: Scale up particles with damage.
	}

	private void UpdateSound()
	{
		_currentLaserSound ??= Sound.Play( LaserLoopSound );
		_currentLaserSound.Position = Transform.Position;
		_currentLaserSound.Pitch = DamagePitchScale.Evaluate( TickDamage.LerpInverse( MinDamage, MaxDamage ) );
	}

	private void UpdateScreenShake()
	{
		var screenShakeAmount = TickDamage.Remap( MinDamage, MaxDamage, 0.12f, 0.20f );
		ScreenEffects.SetBaseScreenShake( this, screenShakeAmount, true );
	}

	private static void OrphanizeEffect( GameObject go )
	{
		var position = go.Transform.Position;
		go.SetParent( null );
		go.Transform.Position = position;
		go.ToggleParticleEmission( false );
		var selfDestruct = go.Components.Create<SelfDestruct>();
		selfDestruct.Delay = 2f;
	}

	private void DestroyLaserEffect()
	{
		_currentLaserEffect?.GameObject?.Destroy();
		_currentLaserEffect = null;
		if ( _laserHitTarget is not null )
		{
			_laserHitTarget.ToggleParticleEmission( false );
			foreach( var light in _laserHitTarget.Components.GetAll<Light>( FindMode.EnabledInSelfAndDescendants ) )
			{
				light.Enabled = false;
			}
			var selfDestruct = _laserHitTarget.Components.Create<SelfDestruct>();
			selfDestruct.Delay = 2f;
			_laserHitTarget = null;
		}
		_currentLaserSound?.Stop( 0.1f );
		_currentLaserSound = null;
	}

	private void StartDamage( GameObject targetGo )
	{
		if ( _untilScreenPunch )
		{
			_untilScreenPunch = 0.5f;
			var screenPunchAmount = TickDamage.Remap( MinDamage, MaxDamage, 0.05f, 0.1f );
			ScreenEffects.AddScreenShake( screenPunchAmount );
		}

		_currentTarget = targetGo;
		_untilNextDamageTick = TickInterval - TickInterval * DamagePhaseOffset;
	}

	private void EndDamage()
	{
		ScreenEffects.ClearBaseScreenShake( this );
		_currentTarget = null;
	}

	private void UpdateDamage( SceneTraceResult tr, IDamageable damageable )
	{
		if ( !_untilNextDamageTick )
			return;

		_untilNextDamageTick = TickInterval;
		var damageInfo = new DamageInfo()
		{
			Attacker = ShipController.GetCurrent().GameObject,
			Damage = TickDamage,
			Position = tr.HitPosition,
			Weapon = GameObject
		};
		DoLaserPush( tr );
		damageable.OnDamage( damageInfo );
	}

	private void DoLaserPush( SceneTraceResult tr )
	{
		if ( !tr.GameObject.Components.TryGet<Rigidbody>( out var rb ) )
			return;

		var force = tr.Direction * 0.2f;
		rb.ApplyImpulse( force * 2500f );
		rb.ApplyImpulseAt( tr.HitPosition, force * 1f );
	}

	private void UpdateBodyRotation()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		var direction = (mousePos - Body.Transform.Position).Normal;
		var yaw = Rotation.LookAt( direction ).Yaw();
		Body.Transform.Rotation = ((Angles)Body.Transform.Rotation).WithYaw( yaw );
	}

	public void OnMakeDebris()
	{
		DestroyLaserEffect();
		Enabled = false;
	}
}