using Sandbox;
using Sandbox.Extensions;
using System.Collections.Generic;
using System.Linq;

public sealed class DroneSpawner : Component
{
	[Property] public GameObject DronePrefab { get; set; }
	[Property] public int DroneCount { get; set; } = 15;
	[Property] public bool SpawnOnStart { get; set; } = true;

	protected override void OnStart()
	{
		if ( SpawnOnStart )
		{
			ForceSpawnMany();
		}
	}

	public GameObject SpawnOne()
	{
		var chunk = GameObject.GetWorldChunk();
		var bounds = chunk.GetChunkWorldBoundsAbsolute();
		var position = bounds.RandomPointInside;
		Log.Info( $"Spawning drone in chunk {chunk} at position: {position}" );
		return DronePrefab.Clone( position );
	}

	[Button( "Force Spawn Many" )]
	public void ForceSpawnMany()
	{
		SpawnMany().ToList();
	}

	public IEnumerable<GameObject> SpawnMany()
	{
		for (int i = 0; i < DroneCount; i++)
		{
			yield return SpawnOne();
		}
	}
}
