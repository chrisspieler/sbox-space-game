using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Extensions
{
	public static class Vector2IntExtensions
	{
		public static BBox GetChunkWorldBoundsAbsolute( this Vector2Int chunkCoords )
		{
			var center = WorldChunker.ChunkCenterToWorldAbsolute( chunkCoords );
			var size = new Vector3( WorldChunker.ChunkSize, WorldChunker.ChunkSize, 0f );
			return BBox.FromPositionAndSize( center, size );
		}
	}
}
