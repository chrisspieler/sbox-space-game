using Sandbox;

[GameResource( "Chunk Data", "chunk", "Chunk metadata", Icon = "landscape" )]
public class ChunkData : GameResource
{
	public string Name { get; set; }
	public PrefabFile Prefab { get; set; }
	public int Difficulty { get; set; }
	public AsteroidSpawnConfig Asteroids { get; set; }

	public GameObject Spawn( Vector3 relativePosition )
	{
		var chunkGo = Prefab.GetPrefabScene().Clone( relativePosition );
		chunkGo.BreakFromPrefab();
		SetChunkData( chunkGo );
		SpawnAsteroids( chunkGo );
		return chunkGo;
	}

	private void SetChunkData( GameObject chunkGo )
	{
		var chunkCell = WorldChunker.WorldToChunkAbsolute( chunkGo.GetAbsolutePosition() );
		var chunkReference = chunkGo.Components.GetOrCreate<ChunkReference>();
		chunkReference.Position = chunkCell;
		chunkReference.Data = this;
		chunkGo.Name = $"{chunkCell} {ResourceName}";
	}

	private void SpawnAsteroids( GameObject chunkGo )
	{
		if ( Asteroids is null )
			return;

		var spawner = chunkGo.Components.GetOrCreate<AsteroidSpawner>();
		spawner.Config = Asteroids;
		spawner.BeginSpawnMany();
	}
}
