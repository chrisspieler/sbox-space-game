using Sandbox;
using Sandbox.Utility;
using System.Collections.Generic;

public sealed class AsteroidSpawner : Component
{
	[ConVar("asteroid_spawn_debug")]
	public static bool Debug { get; set; } = false;

	[Property] public bool SpawnManyOnStart { get; set; } = true;
	[Property] public GameObject AsteroidPrefab { get; set; }
	[Property, Range( 64, 512, 32 )] public int Spacing { get; set; } = 64;
	[Property] public float NoiseScale { get; set; } = 0.1f;
	[Property, Range(0, 1, 0.01f )] public float ProbabilityScale { get; set; } = 0.1f;
	[Property, Range( -1, 1, 0.01f )] public float ProbabilityBias { get; set; } = -0.2f;

	private WorldChunker _chunkSystem;
	private FloatingOriginSystem _originSystem;
	private List<(Vector3 position, float probability)> _spawnPoints = new();

	protected override void OnStart()
	{
		_chunkSystem = Scene.GetSystem<WorldChunker>();
		_originSystem = Scene.GetSystem<FloatingOriginSystem>();

		if ( SpawnManyOnStart )
		{
			SpawnMany();
		}
	}

	public void SpawnOne( Vector3 position)
	{
		var go = AsteroidPrefab.Clone( position );
		go.Parent = GameObject;
	}

	/// <summary>
	/// Returns the localspace position of every point within this chunk that 
	/// could potentially spawn an asteroid.
	/// </summary>
	private IEnumerable<Vector3> GetSpawnPoints()
	{
		var chunkSize = WorldChunker.ChunkSize;
		for (int x = 0; x < chunkSize; x += Spacing)
		{
			for (int y = 0; y < chunkSize; y += Spacing)
			{
				var offset = new Vector3( x, y, Transform.Position.z );
				yield return offset;
			}
		}
	}

	public float GetSpawnProbability( Vector3 localPosition )
	{
		var worldPosition = Transform.World.PointToWorld( localPosition );
		// Make sure that no asteroids spawn within a certain distance of the player spawn.
		// This is to prevent the player from getting telefragged on scene load.
		var originCellCenter = _chunkSystem.ChunkCenterToWorldRelative( Vector2Int.Zero );
		if ( worldPosition.Distance( originCellCenter ) < 1500f )
			return 0f;

		worldPosition = _originSystem.RelativeToAbsolute( worldPosition );
		worldPosition *= NoiseScale;
		var noiseResult = Noise.Perlin( worldPosition.x, worldPosition.y );
		return noiseResult * ProbabilityScale + ProbabilityBias;
	}

	public void SpawnMany()
	{
		var spawnPoints = GetSpawnPoints();
		_spawnPoints = new();
		foreach (var point in spawnPoints)
		{
			var probability = GetSpawnProbability( point );
			// Cache the spawn points so we get better performance during debug draw.
			_spawnPoints.Add( (point, probability) );
			if ( Game.Random.Float() < probability )
			{
				SpawnOne( point );
			}
		}
		if ( Debug )
		{
			Log.Info( $"{GameObject.Name}: Got {_spawnPoints.Count} spawn points." );
		}
	}

	protected override void DrawGizmos()
	{
		if ( !Debug )
			return;

		using ( Gizmo.Scope( "Asteroid Spawn Probability" ) )
		{
			Gizmo.Draw.LineSphere( Vector3.Zero, 200, 8 );
			foreach ( var (point, probability) in _spawnPoints )
			{
				var color = Color.Lerp( Color.Red, Color.Green, probability / ProbabilityScale );
				Gizmo.Draw.Color = color;
				Gizmo.Draw.Line( point, point + Vector3.Up * 20f );
			}
		}
	}
}
