using Sandbox;
using System.Collections.Generic;
using System.Linq;

[GameResource( "World Map", "world", "Defines what chunks can be found in the game world.", Icon = "grid_4x4" )]
public class WorldMap : GameResource
{
	[Property] public ChunkData OriginChunk { get; set; }
	[Property] public List<ChunkData> RandomChunks { get; set; }
	public ChunkData GetChunkDataForCell( Vector2Int cell )
	{
		if ( cell == Vector2Int.Zero )
			return OriginChunk;

		// For now, the difficulty of a chunk in the world is just its chessboard distance from the origin.
		var difficulty = cell.ChebyshevDistance( Vector2Int.Zero );
		var chunks = RandomChunks.Where( c => c.Difficulty <= difficulty );
		if ( chunks.Any() )
		{
			chunks = chunks.OrderByDescending( c => c.Difficulty );
		}
		else
		{
			// If we cannot find an easy enough chunk, find the easiest chunk.
			chunks = RandomChunks.OrderBy( c => c.Difficulty );
		}
		return chunks.First();
	}
}
