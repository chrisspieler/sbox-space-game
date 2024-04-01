using System.Text.Json.Nodes;

/// <summary>
/// A container for holding information about a persisted <see cref="Sandbox.GameObject"/>
/// in a save file.
/// </summary>
public struct PersistedObjectData
{
	public string Type { get; set; }
	/// <summary>
	/// The state that shall be saved to disk. 
	/// </summary>
	public JsonObject Data { get; set; }
}
