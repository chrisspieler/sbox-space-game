public interface IWorldStreamingListener
{
	/// <summary>
	/// Invoked whenever the associated GameObject is removed from the world by
	/// the <see cref="WorldChunker"/>.
	/// </summary>
	/// <param name="wholeChunkUnloaded">
	/// If true, the entire chunk containing the associated GameObject was unloaded.
	/// Otherwise, the GameObject was unloaded becaused it wandered in to a chunk that wasn't
	/// yet loaded.
	/// </param>
	void OnWorldUnload( bool wholeChunkUnloaded );
}
