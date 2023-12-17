using System.Collections.Generic;

using Sandbox;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// Determines how many layers of chunks are loaded in all eight directions from the origin.
	/// </summary>
	public int ChunkLoadDistance { get; set; } = 2;
	public int MaxLoadedChunks { get; set; } = 30;
	public bool DebugDraw { get; set; } = true;

	private readonly Dictionary<Vector2Int, object> _worldChunks = new();
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

	private void LoadChunk( Vector2Int chunk )
	{
		if ( _worldChunks.ContainsKey( chunk ) )
			return;

		_worldChunks[chunk] = new();
		_chunkOrder.Add( chunk );
	}

	private void UnloadChunk( Vector2Int chunk )
	{
		if ( !_worldChunks.ContainsKey( chunk ) )
			return;

		_worldChunks.Remove( chunk );
		_chunkOrder.Remove( chunk );
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
