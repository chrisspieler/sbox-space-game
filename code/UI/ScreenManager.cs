using Sandbox;
using System.Collections.Generic;

public sealed class ScreenManager : Component
{
	public static ScreenManager Instance { get; private set; }

	[Property] public PanelComponent RestartPanel { get; set; }
	[Property] public PanelComponent HudPanel { get; set; }
	[Property] public PanelComponent CursorPanel { get; set; }
	[Property] public PanelComponent HoveredSelectionPanel { get; set; }
	[Property] public PanelComponent ShopPanel { get; set; }
	[Property] public GameObject BeaconContainer { get; set; }
	[Property] public GameObject HealthBarContainer { get; set; }
	[Property] public GameObject TextPanelContainer { get; set; }
	[Property] public PanelComponent PauseMenuPanel { get; set; }

	private Dictionary<IHealth, GameObject> _activeHealthBars = new();

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		if ( Input.EscapePressed )
		{
			ShowPauseMenu();
		}
		GameObject.BreakFromPrefab();
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

	public static void SetBeaconVisibility( bool isVisible )
	{
		Instance.BeaconContainer.Enabled = isVisible;
	}

	public static void AddBeacon( Beacon beacon )
	{
		if ( GetBeacon( beacon ) is not null ) 
			return;

		var panelGo = new GameObject( true, $"Beacon Panel ({beacon?.Name ?? "null"})" );
		panelGo.Parent = Instance.BeaconContainer;
		var beaconPanel = panelGo.Components.Create<BeaconPanel>();
		beaconPanel.Target = beacon;
	}

	public static void RemoveBeacon( Beacon beacon )
	{
		var existing = GetBeacon( beacon );
		if ( existing is null )
			return;

		existing.GameObject.Destroy();
	}

	private static BeaconPanel GetBeacon( Beacon beacon )
	{
		foreach( var child in Instance.BeaconContainer.Children )
		{
			var panel = child.Components.Get<BeaconPanel>();
			if ( panel.IsValid() && panel.Target == beacon )
			{
				return panel;
			}
		}
		return null;
	}

	public static void ShowHealthBar( IHealth health, GameObject target )
	{
		if ( Instance._activeHealthBars.ContainsKey( health ) )
			return;

		var healthBarGo = new GameObject( true, $"Health Bar ({target.Name})" );
		healthBarGo.Parent = Instance.HealthBarContainer;
		var healthBarComponent = healthBarGo.Components.Create<FloatingHealthPanel>();
		healthBarComponent.Target = target;
		healthBarComponent.Health = health;
		Instance._activeHealthBars[health] = healthBarGo;
	}

	public static void RemoveHealthBar( IHealth health )
	{
		if ( !Instance._activeHealthBars.ContainsKey( health ) )
			return;

		var healthBarGo = Instance._activeHealthBars[health];
		healthBarGo.Destroy();
		Instance._activeHealthBars.Remove( health );
	}

	public static void ShowTextPanel( string text, Vector3 position, bool isAlert, float duration = 1.5f )
	{
		var textGo = new GameObject( true, "Text Panel" );
		textGo.Parent = Instance.TextPanelContainer;
		textGo.Transform.Position = position;
		var textComponent = textGo.Components.Create<TextPanel>();
		textComponent.Text = text;
		textComponent.TargetPosition = position;
		textComponent.IsAlert = isAlert;
		textComponent.Lifetime = duration;
	}

	public static void ShowPauseMenu()
	{
		if ( !((PauseMenuPanel)Instance.PauseMenuPanel).CanShowPauseMenu() )
			return;

		Instance.PauseMenuPanel.Enabled = true;
	}
}
