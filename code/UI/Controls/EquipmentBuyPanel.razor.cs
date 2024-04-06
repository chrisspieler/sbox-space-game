using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;

public partial class EquipmentBuyPanel : Panel
{
	public Shop Shop { get; set; }

	private List<Upgrade> _availableEquipment = new();

	private bool CanAfford( Upgrade upgrade ) => Career.Active.HasMoney( upgrade.Cost );

	protected override int BuildHash() => System.HashCode.Combine( Career.Active.Money );

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
		{
			return;
		}
		FetchAvailableEquipment();
	}

	private void FetchAvailableEquipment()
	{
		_availableEquipment = Career.Active
			.GetAvailableEquipment()
			.OrderBy( u => u.Cost )
			.ToList();
		StateHasChanged();
	}

	private void BuyEquipment( Upgrade upgrade )
	{
		Shop.BuyEquipment( ShipController.GetCurrent(), upgrade );
		FetchAvailableEquipment();
		StateHasChanged();
	}
}
