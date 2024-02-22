using Sandbox;
using Sandbox.Utility;
using System.Collections.Generic;

public sealed class AsteroidSpawner : Component
{
	public static bool DebugDraw { get; set; } = false;

	[Property] public bool SpawnManyOnStart { get; set; } = true;
	[Property] public GameObject AsteroidPrefab { get; set; }
	[Property, Range( 64, 512, 32 )] public int Spacing { get; set; } = 64;
	[Property] public float NoiseScale { get; set; } = 0.1f;
	[Property, Range(0, 1, 0.01f )] public float ProbabilityScale { get; set; } = 0.1f;
	[Property, Range( -1, 1, 0.01f )] public float ProbabilityBias { get; set; } = -0.2f;

	protected override void OnStart()
	{
		if ( SpawnManyOnStart )
		{
			SpawnMany();
		}
	}

	

	public void SpawnOne( Transform tx)
	{
		AsteroidPrefab.Clone( tx, GameObject );
	}

	private IEnumerable<Vector3> GetSpawnPoints()
	{
		var chunkSize = WorldChunker.ChunkSize;
		for (int x = -chunkSize; x < chunkSize; x += Spacing)
		{
			for (int y = -chunkSize; y < chunkSize; y += Spacing)
			{
				var offset = new Vector3( x, y, Transform.Position.z );
				yield return Transform.Position + offset;
			}
		}
	}

	public float GetSpawnProbability( Vector3 position )
	{
		// Make sure that no asteroids spawn at the world origin.
		// This is to prevent the player from getting telefragged on scene load.
		if ( position.Length < 1500f )
			return 0f;

		var originSystem = Scene?.GetSystem<FloatingOriginSystem>();
		if ( originSystem is not null )
		{
			position = originSystem.RelativeToAbsolute( position );
		}
		position *= NoiseScale;
		var noiseResult = Noise.Perlin( position.x, position.y );
		return noiseResult * ProbabilityScale + ProbabilityBias;
	}

	public void SpawnMany()
	{
		var spawnPoints = GetSpawnPoints();
		foreach (var point in spawnPoints)
		{
			var probability = GetSpawnProbability( point );
			if ( Game.Random.Float() < probability )
			{
				SpawnOne( new Transform( point ) );
			}
		}
	}

	protected override void DrawGizmos()
	{
		if ( !DebugDraw )
			return;

		var spawnPoints = GetSpawnPoints();
		foreach (var point in spawnPoints)
		{
			var probability = GetSpawnProbability( point );
			var color = Color.Lerp( Color.Red, Color.Green, probability / ProbabilityScale );
			Gizmo.Draw.Color = color; 
			Gizmo.Draw.SolidBox( BBox.FromPositionAndSize( point, 10f ) );
		}
	}
}
