using System;

namespace Sandbox;

public static class Vector2IntExtensions
{
	public static BBox GetChunkWorldBoundsAbsolute( this Vector2Int chunkCoords )
	{
		var center = WorldChunker.ChunkCenterToWorldAbsolute( chunkCoords );
		var size = new Vector3( WorldChunker.ChunkSize, WorldChunker.ChunkSize, 0f );
		return BBox.FromPositionAndSize( center, size );
	}

	public static int RectDistance( this Vector2Int self, Vector2Int other )
		=> Math.Abs( self.x - other.x ) + Math.Abs( self.y - other.y );

	public static int ChebyshevDistance( this Vector2Int self, Vector2Int other )
		=> Math.Max( Math.Abs( self.x - other.x ), Math.Abs( self.y - other.y ) );
}
