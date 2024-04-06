using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

public partial class UpgradeBuyPanel : Panel
{
	public Shop Shop { get; set; }
	private List<RepeatableUpgrade> _availableUpgrades = new();

	private bool CanAfford( RepeatableUpgrade upgrade )
	{
		return Career.Active.HasMoney( Career.Active.GetUpgradeCost( upgrade ) );
	}

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
			.OrderBy( u => u.ResourceName )
			.ToList();
		StateHasChanged();
	}

	private void BuyUpgrade( RepeatableUpgrade upgrade )
	{
		Shop.BuyUpgrade( ShipController.GetCurrent(), upgrade );
		FetchAvailableUpgrades();
		StateHasChanged();
	}
}
