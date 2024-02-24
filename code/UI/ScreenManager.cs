using Sandbox;
using System.Security.Cryptography.X509Certificates;

public sealed class ScreenManager : Component
{
	public static ScreenManager Instance { get; private set; }

	[Property] public PanelComponent RestartPanel { get; set; }
	[Property] public PanelComponent HudPanel { get; set; }
	[Property] public PanelComponent CursorPanel { get; set; }
	[Property] public PanelComponent HoveredSelectionPanel { get; set; }
	[Property] public PanelComponent ShopPanel { get; set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	public static void UpdateShip( ShipController ship )
	{
		var hudPanel = Instance.HudPanel as HudPanel;
		hudPanel.UpdateShip( ship );
	}

	public static void SetHudEnabled( bool enabled )
	{
		Instance.HudPanel.Enabled = enabled;
	}

	public static void SetCursorEnabled( bool enabled )
	{
		Instance.CursorPanel.Enabled = enabled;
	}

	public static void ShowDeathScreen()
	{
		Instance.RestartPanel.Enabled = true;
	}

	public static GameObject GetHoveredSelection()
	{
		SelectionPanel panel = Instance.HoveredSelectionPanel as SelectionPanel;
		return panel?.Target;
	}

	public static void SetHoveredSelection( GameObject selection )
	{
		SelectionPanel panel = Instance.HoveredSelectionPanel as SelectionPanel;
		panel.Target = selection;
		panel.Enabled = selection.IsValid();
	}

	public static bool IsShopOpen()
	{
		return Instance.ShopPanel.Enabled;
	}

	public static void OpenShopPanel( Shop shop )
	{
		ShopPanel shopPanel = Instance.ShopPanel as ShopPanel;
		shopPanel.Shop = shop;
		shopPanel.Enabled = true;
		ShipController.GetCurrent().IsInvincible = true;
	}

	public static void CloseShopPanel()
	{
		Instance.ShopPanel.Enabled = false;
		ShipController.GetCurrent().IsInvincible = ShipController.GodMode;
	}

	public static void SetNearbyShopIndicator( bool isShopNearby )
	{
		(Instance.HudPanel as HudPanel).IsShopNearby = isShopNearby;
	}
}
