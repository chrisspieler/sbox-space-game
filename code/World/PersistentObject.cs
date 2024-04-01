using Sandbox;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// Represents a <see cref="GameObject"/> that may be saved to disk.
/// </summary>
public interface IPersistentObject
{
	IPersistentObjectLoader SaveData();
}

public interface IPersistentObjectLoader
{
	GameObject Load( Vector2Int chunk );
	public PersistedObjectData AsPersistedObjectData()
	{
		return new PersistedObjectData()
		{
			Type = GetType().FullName,
			Data = (JsonObject)JsonSerializer.SerializeToNode(this)
		};
	}
}
