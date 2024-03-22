using Sandbox;
using System;
using System.Collections.Generic;

public sealed class DistantAsteroidEffect : Component, IOriginShiftListener
{
	[ConVar( "distant_asteroid_update_interval" )]
	public static float UpdateIntervalSeconds { get; set; } = 0.25f;

	[Property] public Model AsteroidModel { get; set; }
	[Property] public Material AsteroidMaterial { get; set; }
	[Property] public BBox AsteroidSpawnBounds { get; set; } = BBox.FromPositionAndSize( Vector3.Zero, ( Vector3.One * 5000f ).WithZ( 0f ) );
	[Property] public Color Tint { get; set; } = Color.White;
	[Property] public Curve ScaleCurve { get; set; }

	private List<SceneModel> _asteroids = new();

	private RealTimeSince _sinceLastUpdate;

	protected override void OnUpdate()
	{
		if ( _sinceLastUpdate < UpdateIntervalSeconds )
			return;

		for ( int i = 0; i < _asteroids.Count; i++ )
		{
			var asteroid = _asteroids[i];
			var scaleSpeed = 1f - asteroid.Transform.Scale.x / 20f;
			asteroid.Rotation *= Rotation.FromPitch( 10f * _sinceLastUpdate * scaleSpeed );
			asteroid.Position += Vector3.Right * 20f * _sinceLastUpdate;
			asteroid.Update( _sinceLastUpdate );
			var targetTint = asteroid.ColorTint.WithAlpha( 1f );
			asteroid.ColorTint = Color.Lerp( asteroid.ColorTint, targetTint, Time.Delta / 2f );
		}
		_sinceLastUpdate = 0f;
	}

	public void OnAfterOriginShift( Vector3 offset ) 
	{ 
		foreach( var asteroid in _asteroids )
		{
			asteroid.Position += offset;
		}
	}

	protected override void OnEnabled()
	{
		CreateAsteroids();
		_sinceLastUpdate = 0f;
	}

	protected override void OnDisabled()
	{
		DestroyAsteroids();
	}

	private void CreateAsteroids()
	{
		for ( int i = 0; i < 50; i++ )
		{
			var randomBboxPos = AsteroidSpawnBounds.RandomPointInside;
			var randomPos = Transform.Position + randomBboxPos;
			var tx = new Transform()
				.WithPosition( randomPos )
				.WithRotation( Rotation.Random )
				.WithScale( ScaleCurve.Evaluate( Random.Shared.NextSingle() ) );
			_asteroids.Add( CreateAsteroid( tx ) );
		}
	}

	private void DestroyAsteroids()
	{
		foreach( var asteroid in _asteroids )
		{
			asteroid.Delete();
		}
	}

	private SceneModel CreateAsteroid( Transform tx )
	{
		var model = new SceneModel( Scene.SceneWorld, AsteroidModel, tx );
		model.ColorTint = Tint.WithAlpha( 0f );
		if ( AsteroidMaterial is not null )
		{
			model.SetMaterialOverride( AsteroidMaterial );
		}
		return model;
	}
}
