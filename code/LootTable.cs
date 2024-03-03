using Sandbox;
using System.Collections.Generic;

[GameResource( "Loot Table", "loot", "A list of items that may be dropped.", Icon = "diamond")]
public class LootTable : GameResource
{
	public List<LootItem> Items { get; set; }

	public struct LootItem
	{
		public CargoItem Item { get; set; }
		public float Probability { get; set; }
	}

	public CargoItem Choose()
	{
		var chancer = new RandomChancer<CargoItem>();
		foreach( var item in Items )
		{
			chancer.AddItem( item.Item, item.Probability );
		}
		return chancer.GetNext();
	}
}
