using System.Collections.Generic;

using Sandbox;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// Determines how many layers of chunks are loaded in all eight directions from the origin.
	/// </summary>
	public int ChunkLoadDistance { get; set; } = 3;
	public int MaxLoadedChunks { get; set; } = 80;
	public bool DebugDraw { get; set; } = true;
	public GameObject ChunkContainer { get; private set; }

	private readonly Dictionary<Vector2Int, GameObject> _worldChunks = new();
	private readonly List<Vector2Int> _chunkOrder = new();

	private Vector2Int _previousOrigin = new Vector2Int( -999 );


	public int ChunkCount => _worldChunks.Count;
	public bool IsChunkLoaded( Vector2Int chunk ) => _worldChunks.ContainsKey( chunk );
	public bool IsChunkNearby( Vector2Int chunk, Vector2Int originChunk )
	{
		var rectDistance = chunk.RectDistance( originChunk );

		if ( rectDistance == 0 )
			return true;

		return rectDistance / 2 <= ChunkLoadDistance;
	}

	private void UpdateChunks( Vector2Int originChunk )
	{
		if ( originChunk != _previousOrigin )
		{
			LoadSquare( originChunk, ChunkLoadDistance );
			_previousOrigin = originChunk;
		}
		FreeExcessChunks( originChunk );
		DrawDebugInfo();
	}

	private void EnsureChunkContainer()
	{
		if ( ChunkContainer.IsValid() )
			return;

		ChunkContainer = new GameObject( true, "Streamed Chunks" );
	}

	private void LoadChunk( Vector2Int chunkPos )
	{
		if ( _worldChunks.ContainsKey( chunkPos ) )
			return;

		EnsureChunkContainer();
		var newChunkTx = new Transform()
			.WithPosition( ChunkToWorldRelative( chunkPos ) )
			.WithRotation( Rotation.Identity )
			.WithScale( 1f );
		// TODO: Cache chunks instead of creating new ones every time.
		var prefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/chunks/asteroid_field.prefab" );
		var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
		var chunk = prefabScene.Clone( newChunkTx, ChunkContainer );
		chunk.BreakFromPrefab();
		chunk.Name = $"Chunk {chunkPos}";
		_worldChunks[chunkPos] = chunk;
		_chunkOrder.Add( chunkPos );
	}

	private void UnloadChunk( Vector2Int chunkPos )
	{
		if ( !_worldChunks.ContainsKey( chunkPos ) )
			return;

		var chunk = _worldChunks[chunkPos];
		var children = chunk.Children.ToArray();
		foreach( var child in children )
		{
			child.DestroyImmediate();
		}
		chunk.Destroy();
		_worldChunks.Remove( chunkPos );
		_chunkOrder.Remove( chunkPos );
	}

	private void LoadSquare( Vector2Int origin, int diameter )
	{
		for ( int x = -diameter; x <= diameter; x++ )
		{
			for ( int y = -diameter; y <= diameter; y++ )
			{
				LoadChunk( origin + new Vector2Int( x, y ) );
			}
		}
	}

	private void FreeExcessChunks( Vector2Int originChunk )
	{
		if ( _worldChunks.Count < MaxLoadedChunks )
			return;

		var excess = _worldChunks.Count - MaxLoadedChunks;
		var chunks = _chunkOrder.ToArray();
		foreach( var chunk in chunks )
		{
			if ( !IsChunkNearby( chunk, originChunk ) )
			{
				UnloadChunk( chunk );
				excess--;
				if ( excess <= 0 )
					break;
			}
		}
	}

	private void DrawDebugInfo()
	{
		if ( !DebugDraw )
			return;

		Gizmo.Draw.ScreenText( $"Loaded {ChunkCount}/{MaxLoadedChunks} chunks", new Vector2( 25, 75 ), "Consolas", 12, TextFlag.Left );
		var startPos = new Vector2( 200f, 250f );
		var distance = 7;
		for ( int x = -distance; x <= distance; x++ )
		{
			for ( int y = -distance; y <= distance; y++ )
			{
				var chunk = _previousOrigin - new Vector2Int( x, y );
				var chunkPos = startPos + new Vector2( y * 20f, x * 20f );
				var color = Color.Yellow;
				if ( chunk != _previousOrigin )
				{
					color = _worldChunks.ContainsKey( chunk ) ? Color.Green : Color.Red;
				}
				Gizmo.Draw.Color = color;
				Gizmo.Draw.ScreenText( "■", chunkPos, "Consolas", 24, TextFlag.Left );
			}
		}
	}
}
