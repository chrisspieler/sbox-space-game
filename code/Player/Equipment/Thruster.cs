using Sandbox;

public sealed class Thruster : Component
{
	[ConVar( "thruster_turnaround_boost" )]
	public static float TurnaroundBoost { get; set; } = 1.5f;

	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public ShipController Controller { get; set; }
	[Property] public bool Retrorocket { get; set; } = false;
	[Property] public float Power { get; set; } = 100f;
	[Property] public float FuelConsumptionPerSecond { get; set; } = 0.5f;
	[Property] public bool ShouldFire { get; set; } = false;
	[Property] public float LifetimeScale { get; set; } = 1f;
	private GameObject _effectInstance { get; set; }

	protected override void OnUpdate()
	{
		EnsureEffectInstance();

		var force = Burn();
		Controller.Rigidbody.Velocity += force * Time.Delta;
		var alignment = GetAlignment( force );
		if ( force.IsNearZeroLength || alignment < 0.5f )
		{
			_effectInstance.Enabled = false;
			return;
		}

		UpdateEffect( alignment );
		ShouldFire = false;
	}

	public Vector3 GetForce()
	{
		if ( Retrorocket )
			return Controller.RetrorocketForce;

		// The force that a thruster applies to the ship is the opposite of the direction that the thruster is facing.
		var force = -Power * Transform.Rotation.Forward;
		// Figure out whether the thruster is aligned with the direction in which the ship is currently moving.
		var alignment = force.Normal.Dot( Controller.Rigidbody.Velocity.Normal );
		alignment = (alignment + 1f) / 2f;
		// Add bonus speed if you are trying to stop. Not physically accurate, but feels better.
		var forceScale = alignment.Remap( 0f, 1f, TurnaroundBoost, 1f );
		return force * forceScale;
	}

	private void EnsureEffectInstance()
	{
		if ( _effectInstance.IsValid() )
			return;

		_effectInstance = EffectPrefab.Clone();
		_effectInstance.BreakFromPrefab();
		_effectInstance.Parent = GameObject;
		_effectInstance.Transform.World = Transform.World.WithScale( 1f );
	}

	private Vector3 Burn()
	{
		if ( !ShouldFire || !Controller.Fuel.IsValid() )
			return Vector3.Zero;

		var burnAmount = FuelConsumptionPerSecond * Time.Delta;
		if ( !Controller.Fuel.TryBurnFuel( burnAmount ) )
			return Vector3.Zero;

		return GetForce();
	}

	/// <summary>
	/// Returns a value from 0 to 1 which represents how closely the current
	/// heading of the ship aligns with the force applied by this thruster group.
	/// </summary>
	private float GetAlignment( Vector3 thrusterForce )
	{
		// A thruster is aligned if it is pointed in the opposite direction of its force.
		thrusterForce *= -1f;
		var dot = thrusterForce.Normal.Dot( Transform.Rotation.Forward );
		return (dot + 1f) / 2f;
	}

	private void UpdateEffect( float alignment )
	{
		_effectInstance.Enabled = true;
		var particle = _effectInstance.Components.Get<ParticleEmitter>( true );
		particle.Rate = MathX.Lerp( 1, 50, alignment );
		var effect = _effectInstance.Components.Get<ParticleEffect>();
		effect.Lifetime = MathX.Lerp( 0.1f, 1f, alignment ) * LifetimeScale;
	}

	protected override void OnDisabled()
	{
		_effectInstance?.Destroy();
		_effectInstance = null;
	}

	protected override void OnDestroy()
	{
		_effectInstance?.Destroy();
	}
}
