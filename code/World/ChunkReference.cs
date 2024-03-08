using Sandbox;

public sealed class ChunkReference : Component
{
	[Property] public Vector2Int Position { get; set; }
	[Property] public ChunkData Data { get; set; }
}
