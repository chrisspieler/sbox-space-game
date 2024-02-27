using Sandbox;

public sealed class ChunkData : Component
{
	[Property] public Vector2Int Position { get; set; }

	private WorldChunker _chunkSystem;
	private bool _chunkHasMoved;

	protected override void OnStart()
	{
		_chunkSystem = Scene.GetSystem<WorldChunker>();
	}

	protected override void OnUpdate()
	{
		if ( !_chunkHasMoved )
		{
			Transform.Position = _chunkSystem.ChunkToWorldRelative( Position );
			_chunkHasMoved = true;
		}
	}
}
