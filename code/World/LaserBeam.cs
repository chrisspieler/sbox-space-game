using System.Collections.Generic;

namespace Sandbox;

public sealed class LaserBeam : Component
{
	[ConVar( "laser_light_count" )]
	public static int LaserLightCount { get; set; } = 10;

	[Property] public GameObject Target { get; set; }
	[Property] public Color Tint { get; set; }
	[Property] public ParticleSystem ParticleAsset { get; set; }
	[Property] public LegacyParticleSystem ParticleInstance { get; set; }

	private readonly List<SceneLight> _lights = new();

	protected override void OnStart()
	{
		CreateParticles();
	}

	protected override void OnEnabled()
	{
		CreateLights();
	}

	protected override void OnDisabled()
	{
		DestroyLights();
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
		for ( int i = 0; i < LaserLightCount; i++ )
		{
			var light = new SceneLight( Scene.SceneWorld )
			{
				Radius = 400f,
				ShadowsEnabled = false
			};
			_lights.Add( light );
		}
		UpdateLights();
	}

	private void UpdateLights()
	{
		for ( int i = 0; i < _lights.Count; i++ )
		{
			if ( Target.IsValid() )
			{
				var position = i / (float)LaserLightCount;
				_lights[i].Position = Transform.Position.LerpTo( Target.Transform.Position, position );
			}
			_lights[i].LightColor = Tint.ToHsv().WithValue( 0.5f );
		}
	}

	private void DestroyLights()
	{
		foreach( var light in _lights.ToArray() )
		{
			light.Delete();
		}
		_lights.Clear();
	}

	private void SetControlPoints()
	{
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
				ColorValue = Tint,
				StringCP = "2"
			}
		};
	}
}
