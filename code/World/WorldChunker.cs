using Sandbox;
using System.Linq;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// The size of each world chunk in units. 
	/// </summary>
	[ConVar( "world_chunk_size" )]
	public static int ChunkSize { get; set; } = 5_000;
	[ConVar( "world_streaming" )]
	public static bool EnableWorldStreaming { get; set; } = true;

	public WorldMap World { get; set; }

	// This number is based on a chunk size of 1000 units.
	// Perhaps it should be calculated based on the chunk size.
	private float _maxPosition { get; set; } = 1_000_000_000f;
	private FloatingOriginSystem _originSystem { get; set; }

	public WorldChunker( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, DrawDebugInfo, "World Chunker Draw Debug Info" );
		Listen( Stage.PhysicsStep, 1, OnUpdate, "World Chunker" );
	}

	private void OnUpdate()
	{
		if ( !EnableWorldStreaming || World is null  )
			return;

		_originSystem ??= Scene.GetSystem<FloatingOriginSystem>();

		var origin = _originSystem.Origin;

		if ( !origin.IsValid() )
			return;

		if ( IsPositionTooFar( origin.AbsolutePosition ) )
		{
			origin.AbsolutePosition = Vector3.Zero;
		}

		UpdateChunks( WorldToChunkAbsolute( origin.AbsolutePosition ) );
		if ( Rehoming )
		{
			RehomeDrifters();
			RehomeOrphans();
		}
	}

	/// <summary>
	/// Given an absolute world position, returns the chunk that contains it.
	/// </summary>
	public static Vector2Int WorldToChunkAbsolute( Vector3 absPosition )
	{
		// If we are on the origin of a chunk...
		if ( absPosition.x % ChunkSize < 0.01f || absPosition.y % ChunkSize < 0.01f )
		{
			// ...nudge the position by one unit forward/left to ensure that we are inside of it.
			absPosition += new Vector3( 0.01f, 0.01f, 0 );
		}
		var chunkX = MathX.CeilToInt( absPosition.x / ChunkSize ) - 1;
		var chunkY = MathX.CeilToInt( absPosition.y / ChunkSize ) - 1;
		return new Vector2Int( chunkX, chunkY );
	}

	/// <summary>
	/// Given the coordinates of a chunk, returns the worldspace origin of the chunk.
	/// </summary>
	public static Vector3 ChunkToWorldAbsolute( Vector2Int chunk )
	{
		var chunkScaled = chunk * ChunkSize;
		return new Vector3( chunkScaled.x, chunkScaled.y );
	}

	/// <summary>
	/// Given a relative world position, returns the chunk that contains it.
	/// </summary>
	public Vector2Int WorldToChunkRelative( Vector3 relativePos )
	{
		return WorldToChunkAbsolute( _originSystem.RelativeToAbsolute( relativePos ) );
	}

	/// <summary>
	/// Returns the origin-shifted position of the given chunk.
	/// </summary>
	public Vector3 ChunkToWorldRelative( Vector2Int chunk )
	{
		var absPosition = new Vector3( chunk.x * ChunkSize, chunk.y * ChunkSize, 0f );
		return _originSystem.AbsoluteToRelative( absPosition );
	}

	/// <summary>
	/// Returns the origin-shifted position of the center of the given chunk.
	/// </summary>
	public Vector3 ChunkCenterToWorldRelative( Vector2Int chunk )
	{
		var chunkPos = ChunkToWorldRelative( chunk );
		return chunkPos + new Vector3( 1, 1, 0 ) * ChunkSize / 2;
	}

	public static Vector3 ChunkCenterToWorldAbsolute( Vector2Int chunk )
	{
		return ( ChunkToWorldAbsolute( chunk ) + ChunkSize / 2f ).WithZ( 0f );
	}

	/// <summary>
	/// Checks whether a position is so far from the origin that the step
	/// size of one or more of its components risks exceeding the chunk size.
	/// </summary>
	private bool IsPositionTooFar( Vector3 worldPosition )
	{
		var absPosition = worldPosition.Abs();
		return absPosition.x > _maxPosition || absPosition.y > _maxPosition;
	}
}
