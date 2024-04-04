﻿using Sandbox;

public partial class ShopPanel : PanelComponent
{
	[Property] public Shop Shop { get; set; }

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
}
