/// <summary>
/// Provides a base transform for use with <see cref="OffsetCombiner"/>.
/// </summary>
public interface IBasisSource
{
	/// <summary>
	/// Returns a base transform on to which all offsets may be applied.
	/// </summary>
	/// <param name="lastTransform">The last base transform, without any offest having been applied.</param>
	Transform GetBaseTransform( Transform lastTransform );
}
