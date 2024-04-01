using Sandbox;

public sealed partial class Pickup : Component, IPersistentObject
{
	public IPersistentObjectLoader SaveData()
	{
		return new PickupSaveData()
		{
			Position = Transform.LocalPosition,
			Cargo = Item.ResourcePath
		};
	}
}

public struct PickupSaveData : IPersistentObjectLoader
{
	public Vector2 Position { get; set; }
	public string Cargo { get; set; }

	public readonly GameObject Load( Vector2Int chunk )
	{
		var relativeChunkPos = WorldChunker.ChunkToWorldAbsolute( chunk ).ToRelativePosition();
		var position = relativeChunkPos + (Vector3)Position;
		return Pickup.Spawn( ResourceLibrary.Get<CargoItem>( Cargo ), position )
			.GameObject;
	}
}
