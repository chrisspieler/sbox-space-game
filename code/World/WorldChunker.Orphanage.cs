using Sandbox;
using System.Linq;

public partial class WorldChunker
{
	[ConVar( "world_streaming_rehome" )]
	public static bool Rehoming { get; set; } = true;
	[ConVar( "world_streaming_rehome_interval")]
	public static float RehomeIntervalSeconds { get; set; } = 1f;

	private void RehomeDrifters()
	{
		foreach( var (chunk, chunkGo) in _worldChunks )
		{
			var children = chunkGo.Children.ToList();
			foreach( var child in children )
			{
				var currentChunk = WorldToChunkRelative( child.Transform.Position );
				if ( chunk != currentChunk )
				{
					if ( Debug )
					{
						Log.Info( $"({child.Name}) Rehome {chunk} -> {currentChunk}" );
					}
					AssignToChunk( child, currentChunk );
				}
			}
		}
	}
	private void RehomeOrphans()
	{
		foreach( var go in Scene.GetAllObjects( false ) )
		{
			if ( go.Parent != Scene || go.Tags.Has( "no_chunk" ) )
				continue;

			var chunk = WorldToChunkRelative( go.Transform.Position );
			if ( Debug )
			{
				Log.Info( $"({go.Name}) Rehome null -> {chunk}" );
			}
			AssignToChunk( go, chunk );
		}
	}

	private void AssignToChunk( GameObject go, Vector2Int chunk )
	{
		if ( IsChunkLoaded( chunk ) )
		{
			// Workaround for: https://github.com/Facepunch/sbox-issues/issues/4926
			var formerTransform = go.Transform.World;
			go.Parent = _worldChunks[chunk];
			go.Transform.World = formerTransform;
			go.Transform.ClearInterpolation();
		}
		else
		{
			var listeners = go.Components.GetAll<IWorldStreamingListener>( FindMode.EnabledInSelfAndDescendants );
			foreach ( var listener in listeners )
			{
				listener.OnWorldUnload( wholeChunkUnloaded: false );
			}
			// When an object drifts in to an unloaded area, we don't automatically persist it, as the logic for
			// determining in what chunk (if any) they belong should be handled by the IWorldStreamingListener.
			go.Destroy();
		}
	}
}
