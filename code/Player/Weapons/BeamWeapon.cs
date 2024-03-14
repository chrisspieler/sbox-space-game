using Sandbox;
using System;

public sealed class BeamWeapon : Component
{
	[Property] public Action<BeamWeapon> OnStartFiring { get; set; }
	[Property] public Action<BeamWeapon> OnEndFiring { get; set; }
	[Property] public Action<BeamWeapon, GameObject> OnTargetChanged { get; set; }

	[Property] public BeamTrace Tracer { get; set; }
	[Property] public DamageOverTime Damage { get; set; }
	[Property] public LaserBeam LaserEffect { get; set; }
	[Property] public ImpactEffect Impact { get; set; }
	[Property] public bool ShouldFire 
	{
		get => _shouldFire;
		set
		{
			var oldValue = _shouldFire;
			_shouldFire = value;
			if ( !Game.IsPlaying )
				return;

			if ( value && !oldValue )
			{
				OnStartFiring?.Invoke( this );
			}
			else if ( !value && oldValue )
			{
				OnEndFiring?.Invoke( this );
			}
		}
	}
	private bool _shouldFire;
	[Property] public Curve DistanceCurve { get; set; }
	[Property] public float LaserDistance => DistanceCurve.Evaluate( LaserPower );
	[Property] public Color LaserTint
	{
		get => LaserEffect?.Tint ?? Color.White;
		set
		{
			_laserTint = value;
			UpdateLaserTint();
		}
	}
	private Color _laserTint;
	[Property] public Curve LaserBrightnessCurve { get; set; }

	[Property, Category( "Damage" )] public float LaserPower => MathX.LerpInverse( TickDamage, MinDamage, MaxDamage );
	[Property, Category( "Damage" )]
	public float TickInterval
	{
		get => Damage?.TickInterval ?? 0.5f;
		set
		{
			_tickInterval = value;
			UpdateTickInterval();
		}
	}
	private float _tickInterval;
	[Property, Category("Damage")]
	public float TickDamage
	{
		get => Damage?.TickDamage ?? 0f;
		set
		{
			_tickDamage = value;
			UpdateTickDamage();
		}
	}
	private float _tickDamage;
	
	[Property, Category("Damage")] 
	public float MaxDamage { get; set; } = 20f;
	[Property, Category("Damage")] 
	public float MinDamage { get; set; } = 2.5f;
	[Property, Category("Sound")] 
	public SoundEvent FiringSoundLoop { get; set; }
	[Property, Category( "Sound" )]
	public Curve DamagePitchCurve { get; set; }

	[Property]
	public GameObject Target
	{
		get => _target;
		private set
		{
			var oldValue = _target;
			_target = value;
			if ( !Game.IsPlaying )
				return;

			if ( _target != oldValue )
			{
				OnTargetChanged?.Invoke( this, _target );
			}
		}
	}
	private GameObject _target;

	private SoundHandle _activeFiringSound;

	private void UpdateTickInterval()
	{
		if ( Damage is not null )
		{
			Damage.TickInterval = _tickInterval;
		}
	}

	private void UpdateTickDamage()
	{
		LaserTint = LaserTint.ToHsv().WithValue( LaserBrightnessCurve.Evaluate( LaserPower ) );
		if ( Damage is not null )
		{
			Damage.TickDamage = _tickDamage;
		}
		if ( Tracer is not null )
		{
			Tracer.Distance = LaserDistance;
		}
	}

	private void UpdateLaserTint()
	{
		if ( LaserEffect is not null )
		{
			LaserEffect.Tint = _laserTint;
		}
	}

	public float GetDistanceToTarget()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		return Tracer.Transform.Position.Distance( mousePos );
	}

	protected override void OnEnabled()
	{
		UpdateLaserTint();
		UpdateTickDamage();
		UpdateTickInterval();
		if ( Tracer is not null )
		{
			Tracer.OnHit += Hit;
			Tracer.OnMiss += Miss;
		}
		if ( Damage is not null )
		{
			Damage.AfterDamageTick += AfterDamage;
		}
		if ( LaserEffect is not null )
		{
			LaserEffect.Target = Impact.GameObject;
		}
	}

	protected override void OnDisabled()
	{
		ClearAll();
		if ( Tracer is not null )
		{
			Tracer.OnHit -= Hit;
			Tracer.OnMiss -= Miss;
		}
		if ( Damage is not null )
		{
			Damage.AfterDamageTick -= AfterDamage;
		}
	}

	protected override void OnUpdate()
	{
		if ( !ShouldFire )
		{
			ClearAll();
			return;
		}
		Tracer.SetEnabled( ShouldFire );
		LaserEffect.SetEnabled( ShouldFire );
		UpdateSound();
	}
	
	private void ClearAll()
	{
		Damage.Target = null;
		Impact.Target = null;
		Tracer.SetEnabled( false );
		LaserEffect.SetEnabled( false );
		StopFiringSound();
	}

	private void UpdateSound()
	{
		if ( !_shouldFire )
		{
			StopFiringSound();
			return;
		}

		EnsureFiringSound();
		_activeFiringSound.Position = Tracer.Source.Transform.Position;
		_activeFiringSound.Pitch = DamagePitchCurve.Evaluate( LaserPower );
	}

	private void EnsureFiringSound()
	{
		if ( _activeFiringSound?.IsPlaying is true )
			return;

		_activeFiringSound = Sound.Play( FiringSoundLoop );
	}

	private void StopFiringSound()
	{
		_activeFiringSound?.Stop( 0.1f );
		_activeFiringSound = null;
	}
	
	private void Hit( TraceResult tr )
	{
		Target = tr.GameObject;
		Damage.Target = tr.GameObject;
		Damage.Enabled = true;
		UpdateImpact( tr );
		DoLaserPush( tr );
	}

	private void DoLaserPush( TraceResult tr )
	{
		if ( !tr.GameObject.Components.TryGet<Rigidbody>( out var rb ) )
			return;

		var force = tr.Direction * LaserPower;
		rb.ApplyImpulse( force * 50f );
		rb.ApplyImpulseAt( tr.EndPosition, force * 2f );
	}

	private void Miss( TraceResult tr )
	{
		Target = tr.GameObject;
		Damage.Target = null;
		Damage.Enabled = false;
		UpdateImpact( tr );
	}

	private void UpdateImpact( TraceResult tr )
	{
		if ( Impact is null )
			return;

		Impact.Target = tr.GameObject;
		Impact.Transform.Position = tr.EndPosition;
		Impact.Transform.Rotation = Rotation.LookAt( tr.Normal );
	}

	private void AfterDamage( TargetedDamageInfo damage )
	{

	}
}
