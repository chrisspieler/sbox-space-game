using System;

public struct InputGlyphData
{
	public string ActionName { get; set; }
	public string DisplayText { get; set; }
	public Func<bool> RemovalPredicate { get; set; }
}
