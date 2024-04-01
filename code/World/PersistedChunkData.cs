using System.Collections.Generic;

public struct PersistedChunkData
{
	public PersistedChunkData() { }
	public PersistedChunkData( Vector2 chunkPos, List<PersistedObjectData> children )
	{
		ChunkPosition = chunkPos;
		ChildGameObjects = children;
	}

	public Vector2 ChunkPosition { get; set; }
	public List<PersistedObjectData> ChildGameObjects { get; set; }
}
