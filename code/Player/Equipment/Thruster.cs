using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class Thruster : Component
{
	[ConVar( "thruster_turnaround_boost" )]
	public static float TurnaroundBoost { get; set; } = 1.5f;

	[Property] public List<GameObject> ThrusterGroup { get; set; }
	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public ShipController Controller { get; set; }
	[Property] public bool Retrorocket { get; set; } = false;
	[Property] public float Power { get; set; } = 100f;
	[Property] public float FuelConsumptionPerSecond { get; set; } = 0.5f;
	[Property] public bool ShouldFire { get; set; } = false;
	[Property] public float LifetimeScale { get; set; } = 1f;
	[Property]
	public SoundEvent StartSound { get; set; }
	[Property]
	public SoundEvent LoopSound { get; set; }

	private Dictionary<GameObject, GameObject> _effectInstances { get; set; } = new();
	private SoundHandle _thrusterStartSoundHandle;
	private SoundHandle _thrusterLoopSoundHandle;

	private bool _wasFiringLastUpdate = false;

	protected override void OnUpdate()
	{
		if ( Scene.TimeScale == 0f )
			return;

		EnsureEffectInstance();

		var force = Burn();
		_wasFiringLastUpdate = force != 0f;
		Controller.Rigidbody.Velocity += force * Time.Delta;
		foreach( var ( thruster, effect ) in _effectInstances )
		{
			var alignment = GetAlignment( force, thruster );
			if ( force.IsNearZeroLength || alignment < 0.5f )
			{
				_thrusterLoopSoundHandle?.Stop( 0.1f );
				effect.Enabled = false;
				continue;
			}
			UpdateLoopSound();
			UpdateEffect( effect, alignment );
			ShouldFire = false;
		}
	}

	public Vector3 GetForce()
	{
		if ( Retrorocket )
			return Controller.RetrorocketForce;

		var force = Power * Transform.Rotation.Forward;
		// Figure out whether the thruster is aligned with the direction in which the ship is currently moving.
		var alignment = force.Normal.Dot( Controller.Rigidbody.Velocity.Normal );
		alignment = (alignment + 1f) / 2f;
		// Add bonus speed if you are trying to stop. Not physically accurate, but feels better.
		var forceScale = alignment.Remap( 0f, 1f, TurnaroundBoost, 1f );
		return force * forceScale;
	}

	private void EnsureEffectInstance()
	{
		foreach( var thruster in ThrusterGroup )
		{
			if ( _effectInstances.ContainsKey( thruster ) )
				continue;

			var effect = EffectPrefab.Clone();
			effect.BreakFromPrefab();
			effect.Parent = thruster;
			effect.Transform.World = thruster.Transform.World.WithScale( 1f );
			_effectInstances[thruster] = effect;
		}
	}

	private Vector3 Burn()
	{
		if ( !ShouldFire || !Controller.Fuel.IsValid() )
			return Vector3.Zero;

		var burnAmount = FuelConsumptionPerSecond * Time.Delta;
		if ( !Controller.Fuel.TryBurnFuel( burnAmount ) )
			return Vector3.Zero;

		if ( StartSound is not null && !_wasFiringLastUpdate )
		{
			_thrusterStartSoundHandle?.Stop( 0.1f );
			_thrusterStartSoundHandle = Sound.Play( StartSound, Transform.Position );
		}
		return GetForce();
	}

	/// <summary>
	/// Returns a value from 0 to 1 which represents how closely the current
	/// heading of the ship aligns with the force applied by this thruster group.
	/// </summary>
	private static float GetAlignment( Vector3 thrusterForce, GameObject thruster )
	{
		// A thruster is aligned if it is pointed in the opposite direction of its force.
		thrusterForce *= -1f;
		var dot = thrusterForce.Normal.Dot( thruster.Transform.Rotation.Forward );
		return (dot + 1f) / 2f;
	}

	private void UpdateLoopSound()
	{
		if ( LoopSound is null )
			return;

		if ( _thrusterLoopSoundHandle?.IsPlaying != true )
		{
			_thrusterLoopSoundHandle = Sound.Play( LoopSound );
		}
		_thrusterLoopSoundHandle.Position = Transform.Position;
	}

	private void UpdateEffect( GameObject effectGo, float alignment )
	{
		effectGo.Enabled = true;
		var particle = effectGo.Components.Get<ParticleEmitter>( true );
		particle.Rate = MathX.Lerp( 1, 50, alignment );
		var effect = effectGo.Components.Get<ParticleEffect>();
		effect.Lifetime = MathX.Lerp( 0.1f, 1f, alignment ) * LifetimeScale;
	}

	private void ClearAllEffects()
	{
		foreach ( var (_, effect) in _effectInstances.ToList() )
		{
			effect.Destroy();
		}
		_effectInstances.Clear();
		_thrusterLoopSoundHandle?.Stop( 0.1f );
	}

	protected override void OnDisabled()
	{
		ClearAllEffects();
	}

	protected override void OnDestroy()
	{
		ClearAllEffects();
	}
}
