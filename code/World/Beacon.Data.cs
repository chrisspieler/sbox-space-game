using Sandbox;

public partial class Beacon : Component, IPersistentObject
{
	public IPersistentObjectLoader SaveData()
	{
		return new BeaconSaveData()
		{
			Position = GameObject.GetChunkLocalPosition(),
			Name = Name,
			Id = BeaconId,
			DisappearRange = DisappearRange
		};
	}
}

public struct BeaconSaveData : IPersistentObjectLoader
{
	public Vector2 Position { get; set; }
	public string Name { get; set; }
	public string Id { get; set; }
	public float DisappearRange { get; set; }

	public GameObject Load( Vector2Int chunk )
	{
		var relativeChunkPos = WorldChunker.ChunkToWorldAbsolute( chunk ).ToRelativePosition();
		var position = relativeChunkPos + (Vector3)Position;
		return Beacon.Create( position, Name, Id, DisappearRange ).GameObject;
	}
}
