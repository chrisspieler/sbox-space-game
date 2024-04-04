using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CargoSellPanel : Panel
{
	public Shop Shop { get; set; }

	public override int GetHashCode()
	{
		var ship = ShipController.GetCurrent();
		return HashCode.Combine( ship?.Cargo?.Count );
	}

	private int GetCargoValue( CargoItem item, int quantity )
	{
		return Shop.GetValue( item, quantity );
	}

	private Dictionary<CargoItem, int> GetShipCargoGrouped()
	{
		var allCargo = GetAllShipCargo();
		var cargoTypes = allCargo.Distinct().OrderBy( c => c.BaseValue );
		var grouped = new Dictionary<CargoItem, int>();
		foreach ( var distinct in cargoTypes )
		{
			grouped[distinct] = allCargo.Where( c => c == distinct ).Count();
		}
		return grouped;
	}

	private IEnumerable<CargoItem> GetAllShipCargo()
	{
		var ship = ShipController.GetCurrent();
		if ( !ship.IsValid() || !ship.Cargo.IsValid() )
			return Enumerable.Empty<CargoItem>();

		return ship.Cargo.Items;
	}

	private void SellCargo( CargoItem item )
	{
		var ship = ShipController.GetCurrent();
		Shop.SellItem( item, ship );
	}

	private void SellAllCargo( CargoItem item )
	{
		var ship = ShipController.GetCurrent();
		while ( GetAllShipCargo().Any( c => c == item ) )
		{
			Shop.SellItem( item, ship );
		}
	}
}
