using Sandbox;

public sealed class Thruster : Component
{
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
		return Retrorocket
			? Controller.RetrorocketForce
			: -Power * Transform.Rotation.Forward;
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
		return (dot + 1) / 2;
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
