using Sandbox;
using System.Collections.Generic;
using System.Linq;

public partial class ShopPanel : PanelComponent
{
	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( Career.Active.Money, GetAllShipCargo() );

	[Property] public Shop Shop { get; set; }
	[Property] public ShipController Ship { get; set; }

	private List<Upgrade> _availableUpgrades;

	private bool CanAfford( Upgrade upgrade ) => Career.Active.HasMoney( upgrade.Cost );

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Ship = ShipController.GetCurrent();
		FetchAvailableUpgrades();
	}

	protected override void OnUpdate()
	{
		if ( Input.EscapePressed || !ShipController.GetCurrent().IsValid() )
		{
			Exit();
			return;
		}
	}

	private void Exit()
	{
		Enabled = false;
		Shop.EjectPlayer();
	}

	private IEnumerable<CargoValue> GetAllCargoValues()
	{
		return Shop.GetAllCargoValues();
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
		if ( !Ship.IsValid() || !Ship.Cargo.IsValid() )
			return Enumerable.Empty<CargoItem>();

		return Ship.Cargo.Items;
	}

	private void SellCargo( CargoItem item )
	{
		Shop.SellItem( item, Ship );
	}

	private void SellAllCargo( CargoItem item )
	{
		while ( GetAllShipCargo().Any( c => c == item ) )
		{
			Shop.SellItem( item, Ship );
		}
	}

	private void FetchAvailableUpgrades()
	{
		_availableUpgrades = Career.Active
			.GetAvailableUpgrades()
			.OrderBy( u => u.Cost )
			.ToList();
		StateHasChanged();
	}

	private void BuyUpgrade( Upgrade upgrade )
	{
		Shop.BuyUpgrade( Ship, upgrade );
		FetchAvailableUpgrades();
		StateHasChanged();
	}
}
