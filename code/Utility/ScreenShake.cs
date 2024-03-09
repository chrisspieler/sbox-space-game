using Sandbox;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ScreenShake : Component, IOffsetSource
{
	[ConVar( "camera_shake_scale" )]
	public static float ScreenShakeScale { get; set; } = 0.5f;

	public static ScreenShake Instance { get; private set; }


	[Property, Range( 0f, 1f ), Category( "Screen Shake" )]
	public float Trauma
	{
		get => _trauma;
		set
		{
			_trauma = value.Clamp( 0f, 1f );
		}
	}
	private float _trauma;
	[Property] public float BaseTrauma => _baseTrauma;
	private float _baseTrauma;
	[Property] public float CombinedTrauma => (BaseTrauma + Trauma).Clamp( 0f, 1f );
	[Property] public float TraumaDecayRate { get; set; } = 2f;
	[Property] public float PitchShakeIntensity { get; set; } = 10f;
	[Property] public float PitchShakeSpeed { get; set; } = 100f;
	[Property] public float YawShakeIntensity { get; set; } = 10f;
	[Property] public float YawShakeSpeed { get; set; } = 100f;
	[Property] public float RollShakeIntensity { get; set; } = 10f;
	[Property] public float RollShakeSpeed { get; set; } = 100f;

	[Property] public int ShakeToggleCount => _screenShakeToggles.Count;

	private readonly Dictionary<IValid, float> _screenShakeToggles = new();

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		DecayTrauma();
		UpdateBaseTrauma();
	}

	public Transform GetOffset()
	{
		return new Transform()
			.WithPosition( Vector3.Zero ) // No translational screenshake for now.
			.WithRotation( GetShakeRotation( CombinedTrauma ) );
	}

	public void SetBaseScreenShake( IValid shaker, float amount, bool enabled )
	{
		if ( shaker.IsValid() && enabled && amount > 0f )
		{
			_screenShakeToggles[shaker] = amount;
		}
		else
		{
			// Put the base trauma in to the main trauma as it's removed, to allow
			// the screen shake to smoothly return to normal.
			if ( _screenShakeToggles.ContainsKey( shaker ) )
			{
				Trauma += _screenShakeToggles[shaker];
				Trauma = MathF.Min( 1f, Trauma );
				_screenShakeToggles.Remove( shaker );
			}
		}
	}

	public void ClearAllScreenShakeToggles()
	{
		var totalShake = _screenShakeToggles.Values.Sum();
		Trauma += totalShake;
		Trauma = MathF.Min( 1f, Trauma );
		_screenShakeToggles.Clear();
	}

	private void DecayTrauma()
	{
		Trauma -= Time.Delta * TraumaDecayRate;
	}

	private void UpdateBaseTrauma()
	{
		_baseTrauma = 0f;
		foreach ( var (shaker, trauma) in _screenShakeToggles.ToList() )
		{
			if ( !shaker.IsValid() )
			{
				_screenShakeToggles.Remove( shaker );
				continue;
			}
			_baseTrauma += trauma;
		}
	}

	private Rotation GetShakeRotation( float trauma )
	{
		var pitch = PitchShakeIntensity * Noise.Perlin( Time.Now * PitchShakeSpeed );
		var yaw = YawShakeIntensity * Noise.Perlin( (Time.Now + 1) * YawShakeSpeed );
		var roll = RollShakeIntensity * Noise.Perlin( (Time.Now + 1) * RollShakeSpeed );
		return new Angles( pitch, yaw, roll ) * Easing.EaseIn( trauma ) * ScreenShakeScale;
	}
}
