using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Thruster : Component
{
	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public ShipController Controller { get; set; }
	[Property] public bool Retrorocket { get; set; } = false;
	[Property] public float LifetimeScale { get; set; } = 1f;
	private GameObject _effectInstance { get; set; }

	protected override void OnUpdate()
	{
		var movementValue = Retrorocket
			? -Controller.RetrorocketForce
			: -Controller.MainThrusterForce;
		var dot = movementValue.Normal.Dot( Transform.Rotation.Forward );

		if ( _effectInstance == null )
		{
			_effectInstance = EffectPrefab.Clone();
			_effectInstance.BreakFromPrefab();
			_effectInstance.Parent = GameObject;
			_effectInstance.Transform.World = Transform.World.WithScale( 1f );
		}
		var particle = _effectInstance.Components.Get<ParticleEmitter>( true );
		var normalDot = (dot + 1) / 2;
		if ( normalDot > 0.5f )
		{
			_effectInstance.Enabled = true;
			particle.Rate = MathX.Lerp( 1, 50, normalDot );
			var effect = _effectInstance.Components.Get<ParticleEffect>();
			effect.Lifetime = MathX.Lerp( 0.1f, 1f, normalDot ) * LifetimeScale;
		}
		else
		{
			_effectInstance.Enabled = false;
		}
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
