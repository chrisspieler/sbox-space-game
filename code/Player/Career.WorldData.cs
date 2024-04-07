using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public partial class Career
{
	[JsonPropertyOrder(10)]
	public List<PersistedChunkData> ChunkData { get; set; } = new();

	public Dictionary<Vector2Int, List<PersistedObjectData>> ChunkDataToDictionary()
	{
		var kvp = ChunkData
			.Select( c => new KeyValuePair<Vector2Int, List<PersistedObjectData>>( new Vector2Int( (int)c.ChunkPosition.x, (int)c.ChunkPosition.y ), c.ChildGameObjects ) );
		return new Dictionary<Vector2Int, List<PersistedObjectData>>( kvp );
	}

	public void SaveChunkData( Vector2Int chunkPos, List<PersistedObjectData> chunkData )
	{
		var persistedChunkData = new PersistedChunkData()
		{
			ChunkPosition = new Vector2( chunkPos.x, chunkPos.y ),
			ChildGameObjects = chunkData
		};
		// This would obviously be more efficient if ChunkData were Dictionary with the chunk position
		// as a key, but I couldn't get that to work with the JSON serialization, so I'm not going to bother.
		ChunkData.RemoveAll( c => c.ChunkPosition == persistedChunkData.ChunkPosition );
		ChunkData.Add( persistedChunkData );
	}
}
