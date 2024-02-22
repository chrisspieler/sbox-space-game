using Sandbox;
using System.Linq;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// The size of each world chunk in units. 
	/// </summary>
	[ConVar( "world_chunk_size" )]
	public static int ChunkSize { get; set; } = 5_000;

	// This number is based on a chunk size of 1000 units.
	// Perhaps it should be calculated based on the chunk size.
	private float _maxPosition { get; set; } = 1_000_000_000f;
	private FloatingOriginSystem _originSystem { get; set; }

	public WorldChunker( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, OnUpdate, "World Chunker" );
	}

	private void OnUpdate()
	{
		_originSystem ??= Scene.GetSystem<FloatingOriginSystem>();

		var origin = _originSystem.Origin;

		if ( !origin.IsValid() )
			return;

		if ( IsPositionTooFar( origin.AbsolutePosition ) )
		{
			origin.AbsolutePosition = Vector3.Zero;
		}

		UpdateChunks( WorldToChunkAbsolute( origin.AbsolutePosition ) );
	}

	/// <summary>
	/// Given an absolute world position, returns the chunk that contains it.
	/// </summary>
	public Vector2Int WorldToChunkAbsolute( Vector3 absPosition )
	{
		absPosition.Abs();
		var chunkX = MathX.CeilToInt( absPosition.x / ChunkSize ) - 1;
		var chunkY = MathX.CeilToInt( absPosition.y / ChunkSize ) - 1;
		return new Vector2Int( chunkX, chunkY );
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
		var absPosition = new Vector3( chunk.X * ChunkSize, chunk.Y * ChunkSize, 0f );
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
