using Sandbox.Utility;
using System;
using System.Collections.Generic;

namespace Sandbox;

public sealed class DroneLightManager : Component
{
	[Property] public Drone Controller { get; set; }
	[Property] public List<ModelRenderer> Emissives { get; set; }
	[Property] public List<EffectSpawner> LightDestructionEffects { get; set; }
	[Property] public List<Light> Lights { get; set; }
	[Property] public bool ShouldUpdateLights { get; set; } = true;

	private float _colorSeed;

	protected override void OnValidate()
	{
		Controller ??= Components.Get<Drone>();
	}

	protected override void OnStart()
	{
		_colorSeed = Random.Shared.Float( 0f, 1000f );
	}

	protected override void OnUpdate()
	{
		if ( ShouldUpdateLights )
		{
			UpdateLights();
		}
	}

	public void UpdateLights()
	{
		switch ( Controller.State )
		{
			case Drone.DroneState.Idle:
				UpdateIdleLight( 5f );
				break;
			case Drone.DroneState.Busy:
				UpdateIdleLight( 50f );
				break;
			case Drone.DroneState.Hostile:
				UpdateHostileLight( 15f );
				break;
		}
	}

	private void UpdateHostileLight( float frequency )
	{
		var amplitude = MathF.Sin( Time.Now * frequency );
		amplitude = amplitude.Remap( -1, 1, 0.3f, 0.7f );
		var color = Color.Red * amplitude;
		SetDisplayColor( color );
	}

	private void UpdateIdleLight( float frequency )
	{
		var color = GetColorNoise( frequency ).ToHsv();
		color = color.WithValue( 1f );
		// Make it more of a pastel color.
		color = color.WithSaturation( 0.65f );
		SetDisplayColor( color );
	}

	private Color GetColorNoise( float frequency )
	{
		var color = Color.Black;
		var t = Time.Now + _colorSeed;
		color.r = ( Noise.Perlin( t * frequency ) + 1 ) / 2;
		color.g = ( Noise.Perlin( t + 1 * frequency ) + 1 ) / 2;
		color.b = ( Noise.Perlin( t + 2 * frequency ) + 1 ) / 2;
		return color;
	}

	private void SetDisplayColor( Color color )
	{
		foreach ( var emissive in Emissives )
		{
			emissive.Tint = color;
		}
		foreach( var effect in LightDestructionEffects )
		{
			effect.ParticleTint = color;
		}
		foreach ( var light in Lights )
		{
			light.LightColor = color;
		}
	}
}
