using Sandbox;
using System.Collections.Generic;

public sealed class CargoHold : Component
{
	[Property] public int MaxItems { get; set; } = 6;
	[Property] public List<CargoItem> Items { get; set; } = new();

	public bool CanAddItem( CargoItem item )
	{
		return Items.Count < MaxItems;
	}

	public bool TryAddItem( CargoItem item )
	{
		if ( !CanAddItem( item ) )
			return false;

		Items.Add( item );
		return true;
	}

	public bool RemoveItem( CargoItem item )
	{
		return Items.Remove( item );
	}
}
