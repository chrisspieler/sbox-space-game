using Sandbox;
using System.Collections.Generic;

public sealed class CargoHold : Component
{
	[Property] public int MaxItems { get; set; } = 6;
	[Property] public List<CargoItem> Items { get; set; } = new();

	public int Count => Items.Count;

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

	public bool RemoveAtIndex( int index )
	{
		if ( index < 0 || index > Items.Count - 1 )
			return false;

		Items.RemoveAt( index );
		return true;
	}
}
