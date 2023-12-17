using Sandbox;

public sealed partial class WorldChunker : GameObjectSystem
{
	/// <summary>
	/// The size of each world chunk in units. 
	/// </summary>
	public float ChunkSize { get; set; } = 1000f;

	// This number is based on a chunk size of 1000 units.
	// Perhaps it should be calculated based on the chunk size.
	private float _maxPosition { get; set; } = 1_000_000_000f;

	public WorldChunker( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, OnUpdate, "World Chunker" );
	}

	private void OnUpdate()
	{
		var origin = Scene.GetSystem<FloatingOriginSystem>().Origin;

		if ( !origin.IsValid() )
			return;

		if ( IsPositionTooFar( origin.AbsolutePosition ) )
		{
			origin.AbsolutePosition = Vector3.Zero;
		}

		UpdateChunks( WorldToChunk( origin.AbsolutePosition ) );
	}

	/// <summary>
	/// Given an absolute world position, returns the chunk that contains it.
	/// </summary>
	public Vector2Int WorldToChunk( Vector3 absolutePosition )
	{
		absolutePosition.Abs();
		var chunkX = MathX.CeilToInt( absolutePosition.x / ChunkSize ) - 1;
		var chunkY = MathX.CeilToInt( absolutePosition.y / ChunkSize ) - 1;
		return new Vector2Int( chunkX, chunkY );
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
