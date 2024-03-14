using Sandbox;

[GameResource( "Cargo Item", "cargo", "An item which may be contained within a cargo hold or sold for credits.", Icon = "inventory_2" )]
public class CargoItem : GameResource
{
	public string Name { get; set; }
	[TextArea]
	public string Description { get; set; }
	public int MinValue { get; set; } = 2;
	public int BaseValue { get; set; } = 5;
	public int MaxValue { get; set; } = 7;
	public int ValueDecreaseOnSale { get; set; } = 1;
	/// <summary>
	/// How many other cargo items must be sold before the current value of this item will increase.
	/// </summary>
	public int OtherSalesUntilValueIncrease { get; set; } = 1;
	public int ValueIncreaseAmount { get; set; } = 1;
	public PrefabFile PickupPrefab { get; set; }
	public Texture InventoryIcon { get; set; }
}
