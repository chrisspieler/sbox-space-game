using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

public partial class EquipmentBuyPanel : Panel
{
	public Shop Shop { get; set; }

	private List<Upgrade> _availableUpgrades = new();

	private bool CanAfford( Upgrade upgrade ) => Career.Active.HasMoney( upgrade.Cost );

	protected override int BuildHash() => System.HashCode.Combine( Career.Active.Money );

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
		{
			return;
		}
		FetchAvailableUpgrades();
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
		Shop.BuyUpgrade( ShipController.GetCurrent(), upgrade );
		FetchAvailableUpgrades();
		StateHasChanged();
	}
}
