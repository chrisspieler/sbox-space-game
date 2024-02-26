using Sandbox;

public sealed class ChunkData : Component
{
	[Property] public Vector2Int Position { get; set; }

	private WorldChunker _chunkSystem;

	protected override void OnStart()
	{
		_chunkSystem = Scene.GetSystem<WorldChunker>();
	}

	protected override void OnUpdate()
	{
		Transform.Position = _chunkSystem.ChunkToWorldRelative( Position );
	}
}
