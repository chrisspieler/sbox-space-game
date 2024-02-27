using Sandbox;

[GameResource( "Cargo Item", "cargo", "An item which may be contained within a cargo hold or sold for credits.", Icon = "inventory_2" )]
public class CargoItem : GameResource
{
	public string Name { get; set; }
	[TextArea] 
	public string Description { get; set; }
	public int Value { get; set; }
	public GameObject PickupPrefab { get; set; }
	public Texture InventoryIcon { get; set; }
}
