using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Sandbox;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// Determines how many layers of chunks are loaded in all eight directions from the origin.
	/// </summary>
	[ConVar("world_streaming_depth")]
	public static int ChunkLoadDistance { get; set; } = 1;
	[ConVar("world_streaming_max_loaded_chunks")]
	public static int MaxLoadedChunks { get; set; } = 9;

	[ConVar("world_streaming_debug")]
	public static bool Debug { get; set; }
	public GameObject ChunkContainer { get; private set; }

	private readonly Dictionary<Vector2Int, GameObject> _worldChunks = new();
	private readonly List<Vector2Int> _chunkOrder = new();

	private Dictionary<Vector2Int, List<PersistedObjectData>> _chunkCache = new();

	private Vector2Int _previousOrigin = new Vector2Int( -999 );


	public int ChunkCount => _worldChunks.Count;
	public bool IsChunkLoaded( Vector2Int chunk ) => _worldChunks.ContainsKey( chunk );
	public bool IsChunkNearby( Vector2Int chunk, Vector2Int originChunk )
	{
		return Math.Abs( chunk.x - originChunk.x ) <= ChunkLoadDistance
			&& Math.Abs( chunk.y - originChunk.y ) <= ChunkLoadDistance;
	}

	private void UpdateChunks( Vector2Int originChunk )
	{
		if ( originChunk != _previousOrigin )
		{
			LoadSquare( originChunk, ChunkLoadDistance );
			_previousOrigin = originChunk;
		}
		FreeExcessChunks( originChunk );
	}

	public void Clear()
	{
		foreach( var chunk in _worldChunks.Keys.ToList() )
		{
			UnloadChunk( chunk );
		}
		_chunkCache.Clear();
		_previousOrigin = new Vector2Int( -999 );
	}



	private void EnsureChunkContainer()
	{
		if ( ChunkContainer.IsValid() )
			return;

		ChunkContainer = new GameObject( true, "Streamed Chunks" );
		ChunkContainer.Tags.Add( "no_chunk" );
	}

	private GameObject LoadChunkGameObject( Vector2Int chunkPos )
	{
		if ( TryLoadChunkFromCache( chunkPos, out GameObject chunk ) )
		{
			return chunk;
		}
		chunk = LoadChunkFromCoordinates( chunkPos, true );
		return chunk;
	}
	private bool TryLoadChunkFromCache( Vector2Int chunkPos, out GameObject chunk )
	{
		if ( !_chunkCache.ContainsKey( chunkPos ) )
		{
			chunk = null;
			return false;
		}

		chunk = LoadChunkFromCoordinates( chunkPos, false );
		foreach( var child in _chunkCache[chunkPos] )
		{
			var type = TypeLibrary.GetType<IPersistentObjectLoader>( child.Type );
			var loader = (IPersistentObjectLoader)JsonSerializer.Deserialize( child.Data, type.TargetType );
			var childGo = loader.Load( chunkPos );
			if ( Debug ) { Log.Info( $"{chunkPos} load persisted {childGo.Name} at {childGo.Transform.Position}" ); }
		}
		_chunkCache.Remove( chunkPos );
		return true;
	}

	private GameObject LoadChunkFromCoordinates( Vector2Int chunkPos, bool firstTime )
	{
		var chunkData = World.GetChunkDataForCell( chunkPos );
		var relativePos = ChunkToWorldRelative( chunkPos );
		return chunkData.Generate( relativePos, firstTime );
	}

	private void LoadChunk( Vector2Int chunkPos )
	{
		if ( Debug ) Log.Info( $"Loading chunk: {chunkPos}" );

		if ( _worldChunks.ContainsKey( chunkPos ) )
			return;

		EnsureChunkContainer();
		
		var chunk = LoadChunkGameObject( chunkPos );
		var oldPos = chunk.Transform.Position;
		chunk.Parent = ChunkContainer;
		chunk.Transform.Position = oldPos;
		// For some reason, chunks outside the starting area don't fit to the grid, instead
		// overlapping each other like roof tiles. Moving the chunks after they spawn seems
		// to fix this absurd issue, and I don't know why.
		var mover = chunk.Components.Create<MoveOnSpawn>();
		mover.AbsolutePosition = ChunkToWorldAbsolute( chunkPos );
		_worldChunks[chunkPos] = chunk;
		_chunkOrder.Add( chunkPos );
		if ( Debug ) Log.Info( $"Loaded chunk {chunkPos} at relative position {chunk.Transform.Position}" );
	}

	private void UnloadChunk( Vector2Int chunkPos )
	{
		if ( !_worldChunks.ContainsKey( chunkPos ) )
		{
			if ( Debug ) { Log.Info( $"Chunk not loaded: {chunkPos}" ); }
			return;
		}	

		var chunk = _worldChunks[chunkPos];
		if ( Debug ) { Log.Info( $"Unloading chunk with {chunk.Children.Count} children: {chunkPos}" ); }
		CacheChunk( chunkPos, chunk );
		var children = chunk.Children.ToArray();
		foreach( var child in children )
		{
			UnloadGameObject( child );
			child.DestroyImmediate();
		}
		chunk.Destroy();
		_worldChunks.Remove( chunkPos );
		_chunkOrder.Remove( chunkPos );

		static void UnloadGameObject( GameObject go )
		{
			var listeners = go.Components.GetAll<IWorldStreamingListener>( FindMode.EnabledInSelfAndDescendants );
			foreach ( var listener in listeners )
			{
				listener.OnWorldUnload( wholeChunkUnloaded: true );
			}
		}
	}

	/// <summary>
	/// Overrides the in-memory chunk cache with the specified chunks. Used by the 
	/// <see cref="SaveManager"/> to initialize the chunk cache on game start.
	/// </summary>
	public void LoadChunkCache( Dictionary<Vector2Int, List<PersistedObjectData>> chunkCache )
	{
		_chunkCache = chunkCache;
	}

	/// <summary>
	/// Refreshes chunk cache entries for all loaded chunks. If a loaded chunk is never
	/// unloaded over the course of the game, this is the only way that chunk's data might
	/// be persisted.
	/// </summary>
	public void RefreshChunkCache()
	{
		foreach( var chunk in _worldChunks )
		{
			CacheChunk( chunk.Key, chunk.Value );
		}
	}

	/// <summary>
	/// Overwrites the chunk cache entry for the specified chunk.
	/// </summary>
	private void CacheChunk( Vector2Int chunkPos, GameObject chunk )
	{
		var children = new List<PersistedObjectData>();
		foreach ( var child in chunk.Children )
		{
			if ( !child.Components.TryGet<IPersistentObject>( out var persistent ) )
				continue;
			if ( Debug ) { Log.Info( $"{chunkPos} persisting {child.Name} at {child.Transform.Position}" ); }
			var loader = persistent.SaveData();
			var type = loader.GetType();
			var data = new PersistedObjectData()
			{
				Type = type.FullName,
				Data = (JsonObject)JsonSerializer.SerializeToNode( loader, type )
			};
			children.Add( data );
		}
		_chunkCache[chunkPos] = children;
		// Don't actually save the chunk cache to disk unless the currently loaded world
		// is the one specified by the save file.
		if ( Career.Active?.World == World.ResourceName )
		{
			Career.Active.SaveChunkData( chunkPos, children );
		}
	}

	private void LoadSquare( Vector2Int origin, int diameter )
	{
		if ( Debug ) { Log.Info( $"Load {diameter} diameter square around: {origin}" ); }

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
		foreach ( var chunk in chunks )
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
		if ( !Debug )
			return;

		Gizmo.Draw.ScreenText( $"Loaded {ChunkCount}/{MaxLoadedChunks} chunks", new Vector2( 25, 75 ), "Consolas", 12, TextFlag.Left );
		var startPos = new Vector2( 200f, 250f );
		var distance = ChunkLoadDistance + 2;
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
				Gizmo.Draw.Color = Color.White;
				var difficulty = chunk.ChebyshevDistance( Vector2Int.Zero );
				Gizmo.Draw.ScreenText( difficulty.ToString(), chunkPos + new Vector2(5), "Consolas" );
			}
		}
		foreach( var (coords, _) in _worldChunks )
		{
			Gizmo.Draw.Color = Color.Yellow;
			var position = ChunkCenterToWorldRelative( coords );
			Gizmo.Draw.Text( coords.ToString(), new Transform( position ), "Consolas" );
			var bbox = BBox.FromPositionAndSize( position, new Vector3( ChunkSize ).WithZ( 1f ) );
			Gizmo.Draw.LineBBox( bbox );
		}
		var player = _originSystem?.Origin;
		if ( player.IsValid() )
		{
			var chunk = WorldToChunkRelative( player.Transform.Position );
			Gizmo.Draw.Text( chunk.ToString(), player.Transform.World, "Consolas" );
		}
	}
}
