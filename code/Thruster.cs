﻿using Sandbox;
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
			_effectInstance = SceneUtility.Instantiate( EffectPrefab );
			_effectInstance.Parent = GameObject;
			_effectInstance.Transform.Position = Transform.Position;
			_effectInstance.Transform.Rotation = Transform.Rotation;
		}
		var particle = _effectInstance.Components.Get<ParticleEmitter>( true );
		var normalDot = (dot + 1) / 2;
		if ( normalDot > 0.5f )
		{
			particle.Enabled = true;
			particle.Rate = MathX.Lerp( 20, 250, normalDot );
			var effect = _effectInstance.Components.Get<ParticleEffect>();
			var lifetimeLerped = MathX.LerpInverse( movementValue.Length, 0, 5000 );
			effect.Lifetime = MathX.Lerp( 0.1f, 1f, lifetimeLerped ) * LifetimeScale;
		}
		else
		{
			particle.Enabled = false;
		}
	}

	protected override void OnDisabled()
	{
		_effectInstance?.Destroy();
		_effectInstance = null;
	}
}
