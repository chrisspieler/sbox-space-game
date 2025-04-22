using Sandbox.Utility;
using System.Collections.Generic;

namespace Sandbox;

public sealed class LaserBeam : Component
{
	[ConVar( "laser_light_brightness_scale" )]
	public static float BrightnessScale { get; set; } = 1f;

	[Property] public GameObject Target { get; set; }
	[Property] public Color Tint { get; set; }
	[Property] public ParticleSystem ParticleAsset { get; set; }
	[Property] public LegacyParticleSystem ParticleInstance { get; set; }

	private SceneLight _laserLight;

	protected override void OnStart()
	{
		CreateParticles();
	}

	protected override void OnEnabled()
	{
		CreateLights();
		ParticleInstance.SetEnabled( true );
	}

	protected override void OnDisabled()
	{
		DestroyLights();
		ParticleInstance.SetEnabled( false );
	}

	protected override void OnUpdate()
	{
		SetControlPoints();
		UpdateLights();
	}

	private void CreateParticles()
	{
		ParticleInstance ??= Components.GetOrCreate<LegacyParticleSystem>( FindMode.EverythingInSelf );
		ParticleInstance.Particles = ParticleAsset;
		SetControlPoints();
		ParticleInstance.Enabled = true;
	}

	private void CreateLights()
	{
		DestroyLights();
		_laserLight = new SceneLight( Scene.SceneWorld )
		{
			QuadraticAttenuation = 0.1f,
			FogLighting = SceneLight.FogLightingMode.Dynamic,
			ShadowsEnabled = false,
			Shape = SceneLight.LightShape.Rectangle
		};
		UpdateLights();
	}

	private void UpdateLights()
	{
		var lightLength = Target.IsValid() 
			? WorldPosition.Distance( Target.WorldPosition )
			: 400f;
		_laserLight.Radius = lightLength * 2f;
		_laserLight.FogStrength = 0.3f;
		_laserLight.QuadraticAttenuation = 0.1f;
		_laserLight.Position = WorldPosition;
		_laserLight.Rotation = WorldRotation;
		_laserLight.ShapeSize = new Vector2( 1, lightLength );
		_laserLight.LightColor = Tint.ToHsv().WithValue( 0.5f * BrightnessScale );
	}

	private void DestroyLights()
	{
		_laserLight?.Delete();
		_laserLight = null;
	}

	private void SetControlPoints()
	{
		var tintHsv = Tint.ToHsv();
		var adjustedTint = tintHsv.WithValue( tintHsv.Value * BrightnessScale );
		ParticleInstance.ControlPoints = new()
		{
			new ParticleControlPoint()
			{
				GameObjectValue = GameObject,
				StringCP = "0"
			},
			new ParticleControlPoint()
			{
				GameObjectValue = Target,
				StringCP = "1"
			},
			new ParticleControlPoint()
			{
				Value = ParticleControlPoint.ControlPointValueInput.Color,
				ColorValue = adjustedTint,
				StringCP = "2"
			}
		};
	}
}
