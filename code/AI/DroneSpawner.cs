using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class DroneSpawner : Component
{
	[ConVar( "drone_spawn_debug" )]
	public static bool Debug { get; set; } = false;
	[ConVar( "drone_spawn_rate_limit" )]
	public static int SpawnRateLimit { get; set; } = 1;
	[ConVar( "drone_spawn_factor" )]
	public static float SpawnFactor { get; set; } = 1f;

	[Property] public GameObject DronePrefab { get; set; }
	[Property] public int DroneCount { get; set; } = 15;
	[Property] public bool SpawnOnStart { get; set; } = false;

	private IEnumerator<GameObject> _spawnEnumerator;

	protected override void OnStart()
	{
		DronePrefab ??= ResourceLibrary.Get<PrefabFile>( "prefabs/drones/mining_drone.prefab" ).GetPrefabScene();
		if ( SpawnOnStart )
		{
			BeginSpawnMany();
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
				_spawnEnumerator = null;
				break;
			}
		}
	}

	public void BeginSpawnMany()
	{
		_spawnEnumerator = SpawnMany().GetEnumerator();
	}

	public GameObject SpawnOne()
	{
		var chunk = GameObject.GetWorldChunk();
		var bounds = chunk.GetChunkWorldBoundsAbsolute();
		var position = bounds.RandomPointInside.ToRelativePosition();
		if ( Debug )
		{
			Log.Info( $"({GameObject.Name} {GameObject.Transform.Position}): Spawning drone in chunk {chunk} at position: {position}" );
		}
		return DronePrefab.Clone( position );
	}

	[Button( "Force Spawn Many" )]
	public void ForceSpawnMany()
	{
		SpawnMany().ToList();
	}

	public IEnumerable<GameObject> SpawnMany()
	{
		var droneCount = (int)(DroneCount * SpawnFactor);
		for (int i = 0; i < droneCount; i++)
		{
			yield return SpawnOne();
		}
	}

	[ConCmd("drone_spawn"), Cheat]
	public static void SpawnCommand()
	{
		if ( Game.ActiveScene is null || Game.ActiveScene.Camera is null )
			return;

		var worldPos = Game.ActiveScene.Camera.ScreenNormalToWorld( 0.5f );
		var spawnerGo = new GameObject( true, "Drone Spawner" );
		spawnerGo.Transform.Position = worldPos;
		var spawner = spawnerGo.Components.Create<DroneSpawner>();
		spawner.BeginSpawnMany();
	}
}
