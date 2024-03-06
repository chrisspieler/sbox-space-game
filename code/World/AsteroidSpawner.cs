using Sandbox;
using Sandbox.Utility;
using System.Collections.Generic;
using System.Linq;

public sealed class AsteroidSpawner : Component
{
	[ConVar("asteroid_spawn_debug")]
	public static bool Debug { get; set; } = false;
	[ConVar( "asteroid_spawn_rate_limit" )]
	public static int SpawnRateLimit { get; set; } = 1;

	[Property] public bool SpawnManyOnStart { get; set; } = true;
	[Property] public GameObject AsteroidPrefabC { get; set; }
	[Property] public float CTypeWeight { get; set; } = 100f;
	[Property] public GameObject AsteroidPrefabS { get; set; }
	[Property] public float STypeWeight { get; set; } = 20f;
	[Property] public GameObject AsteroidPrefabM { get; set; }
	[Property] public float MTypeWeight { get; set; } = 10f;
	[Property, Range( 64, 512, 32 )] public int Spacing { get; set; } = 64;
	[Property] public float NoiseScale { get; set; } = 0.1f;
	[Property, Range(0, 1, 0.01f )] public float ProbabilityScale { get; set; } = 0.1f;
	[Property, Range( -1, 1, 0.01f )] public float ProbabilityBias { get; set; } = -0.2f;

	private WorldChunker _chunkSystem;
	private FloatingOriginSystem _originSystem;
	private List<(Vector3 position, float probability)> _spawnPoints = new();
	private RandomChancer<GameObject> _asteroidProbabilities = new();
	private SpawnPoint _playerSpawn;

	private IEnumerator<GameObject> _spawnEnumerator;
	private int _spawnCount = 0;
	private int _spawnUpdateCount = 0;

	protected override void OnStart()
	{
		_chunkSystem = Scene.GetSystem<WorldChunker>();
		_originSystem = Scene.GetSystem<FloatingOriginSystem>();
		_playerSpawn = Scene.GetAllComponents<SpawnPoint>().FirstOrDefault();
		_asteroidProbabilities.AddItem( AsteroidPrefabC, CTypeWeight );
		_asteroidProbabilities.AddItem( AsteroidPrefabS, STypeWeight );
		_asteroidProbabilities.AddItem( AsteroidPrefabM, MTypeWeight );
		if ( Components.TryGet<ChunkData>( out var data ) )
		{
			Transform.Position = _chunkSystem.ChunkToWorldRelative( data.Position );
		}

		if ( SpawnManyOnStart )
		{
			_spawnEnumerator = SpawnMany();
		}
	}

	protected override void OnUpdate()
	{
		if ( _spawnEnumerator is null )
			return;

		for ( int i = 0; i < SpawnRateLimit; i++ )
		{
			if ( !_spawnEnumerator.MoveNext() )
			{
				if ( Debug )
				{
					Log.Info( $"Spawned {_spawnCount} asteroids over {_spawnUpdateCount} updates." );
				}
				_spawnEnumerator = null;
				break;
			}
			_spawnCount++;
		}
		_spawnUpdateCount++;
	}

	public GameObject SpawnOne( Vector3 position)
	{
		var prefab = _asteroidProbabilities.GetNext();
		var go = prefab.Clone();
		go.Parent = GameObject;
		go.Transform.LocalPosition = position;
		return go;
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
		if ( _playerSpawn.IsValid() )
		{
			// Make sure that no asteroids spawn within a certain distance of the player spawn.
			// This is to prevent the player from getting telefragged on scene load.
			if ( worldPosition.Distance( _playerSpawn.Transform.Position ) < 2500f )
				return 0f;
		}
		

		worldPosition = _originSystem.RelativeToAbsolute( worldPosition );
		worldPosition *= NoiseScale;
		var noiseResult = Noise.Perlin( worldPosition.x, worldPosition.y );
		return noiseResult * ProbabilityScale + ProbabilityBias;
	}

	public IEnumerator<GameObject> SpawnMany()
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
				yield return SpawnOne( point );
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

		var chunkPos = GameObject.GetWorldChunk();
		var chunkCenter = _chunkSystem.ChunkCenterToWorldRelative( chunkPos );
		chunkCenter = Transform.World.PointToLocal( chunkCenter );
		Gizmo.Draw.Text( chunkPos.ToString(), new Transform( chunkCenter ), "Consolas" );
		var bboxSize = new Vector3( WorldChunker.ChunkSize, WorldChunker.ChunkSize, 200f );
		var bbox = BBox.FromPositionAndSize( chunkCenter, bboxSize );
		Gizmo.Hitbox.BBox( bbox );

		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
			return;

		Gizmo.Draw.Color = Gizmo.IsSelected
			? Color.White
			: Color.Gray;
		Gizmo.Draw.LineBBox( bbox );

		using ( Gizmo.Scope( "Asteroid Spawn Probability" ) )
		{
			foreach ( var (point, probability) in _spawnPoints )
			{
				var color = Color.Lerp( Color.Red, Color.Green, probability / ProbabilityScale );
				Gizmo.Draw.Color = color;
				Gizmo.Draw.Line( point, point + Vector3.Up * 20f );
			}
		}
	}
}
