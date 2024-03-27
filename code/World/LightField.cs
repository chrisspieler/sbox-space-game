using Sandbox;
using Sandbox.Utility;
using System.Collections.Generic;

public sealed class LightField : Component, IOriginShiftListener
{
	[Property] public float Radius { get; set; } = 2500f;
	[Property] public int LightCount { get; set; } = 20;
	[Property] public float ColorNoiseScale { get; set; } = 0.003f;
	[Property, Range(0,1, 0.01f)] public float ColorValue { get; set; } = 0.05f;

	private List<SceneLight> _lights = new();

	protected override void DrawGizmos()
	{
		foreach( var light in _lights )
		{
			Gizmo.Draw.Color = light.LightColor;
			var position = Transform.World.PointToLocal( light.Position );
			Gizmo.Draw.LineSphere( position, 5f );
		}
	}

	protected override void OnEnabled()
	{
		CreateLights();
	}

	protected override void OnDisabled()
	{
		DestroyLights();
	}

	private void CreateLights()
	{
		DestroyLights();
		var bbox = BBox.FromPositionAndSize( Vector3.Zero, new Vector3( Radius * 2 ).WithZ( 0f ) );
		for (int i = 0; i < LightCount; i++)
		{
			var randomLocalPos = bbox.RandomPointInside;
			var absolutePos = Transform.World.PointToWorld( randomLocalPos ).ToAbsolutePosition();
			_lights.Add( CreateLight( absolutePos ) );
		}
	}

	private SceneLight CreateLight( Vector3 position )
	{
		var color = ColorFromNoise( position )
			.WithValue( ColorValue );
		var so = new SceneLight( Scene.SceneWorld );
		so.Position = position.ToRelativePosition();
		so.Radius = Radius * 1.5f;
		so.LightColor = color;
		so.QuadraticAttenuation = 0.025f;
		so.ShadowsEnabled = false;
		return so;
	}

	private ColorHsv ColorFromNoise( Vector3 position )
	{

		var r = Noise.Perlin( position.x * ColorNoiseScale, position.y * ColorNoiseScale );
		var g = Noise.Perlin( position.x * ColorNoiseScale + 10_000, position.y * ColorNoiseScale + 10_000 );
		var b = Noise.Perlin( position.x * ColorNoiseScale + 20_000, position.y * ColorNoiseScale + 20_000 );
		ColorHsv color = new Color( r, g, b );
		var saturation = Noise.Perlin( position.x * ColorNoiseScale + 30_000, position.y * ColorNoiseScale + 30_000 );
		return color.WithSaturation( saturation );
	}

	private void DestroyLights()
	{
		foreach( var light in _lights )
		{
			light.Delete();
		}
		_lights.Clear();
	}

	public void OnAfterOriginShift( Vector3 offset ) 
	{ 
		foreach( var light in _lights )
		{
			light.Position += offset;
		}
	}
}
