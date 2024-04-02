using Sandbox;
using System.Collections.Generic;
using System.Net.WebSockets;

public sealed partial class ScreenManager : Component
{
	public static ScreenManager Instance { get; private set; }

	[Property] public RestartPanel RestartPanel { get; set; }
	[Property] public HudPanel HudPanel { get; set; }
	[Property] public CursorPanel CursorPanel { get; set; }
	[Property] public SelectionPanel HoveredSelectionPanel { get; set; }
	[Property] public ShopPanel ShopPanel { get; set; }
	[Property] public GameObject HealthBarContainer { get; set; }
	[Property] public GameObject TextPanelContainer { get; set; }
	[Property] public PauseMenuPanel PauseMenuPanel { get; set; }
	[Property] public QtDriveHudPanel QtDriveHud { get; set; }

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
			SaveManager.SaveActiveCareer();
		}
		GameObject.BreakFromPrefab();
	}

	public static void UpdateShip( ShipController ship )
	{
		Instance.HudPanel.UpdateShip( ship );
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
		return Instance.HoveredSelectionPanel?.Target;
	}

	public static void AddSelectionGlyph( InputGlyphData glyphData )
	{
		Instance.HoveredSelectionPanel.AddGlyph( glyphData );
	}

	public static void RemoveSelectionGlyph( string actionName )
	{
		Instance.HoveredSelectionPanel.RemoveGlyph( actionName );
	}

	public static void SetHoveredSelection( GameObject selection )
	{
		Instance.HoveredSelectionPanel.Target = selection;
		Instance.HoveredSelectionPanel.Enabled = selection.IsValid();
	}

	public static bool IsShopOpen()
	{
		return Instance.ShopPanel.Enabled;
	}

	public static void OpenShopPanel( Shop shop )
	{
		Instance.ShopPanel.Shop = shop;
		Instance.ShopPanel.Enabled = true;
		ShipController.GetCurrent().IsInvincible = true;
	}

	public static void CloseShopPanel()
	{
		Instance.ShopPanel.Enabled = false;
		ShipController.GetCurrent().IsInvincible = ShipController.GodMode;
	}

	public static void ShowHealthBar( IHealth health, GameObject target )
	{
		if ( !Instance.IsValid() )
			return;

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
		if ( !Instance.IsValid() )
			return;

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
		textComponent.Target = position;
		textComponent.IsAlert = isAlert;
		textComponent.Lifetime = duration;
	}

	public static void ShowPauseMenu()
	{
		if ( !Instance.PauseMenuPanel.CanShowPauseMenu() )
			return;

		Instance.PauseMenuPanel.Enabled = true;
	}

	public static void SetQtHudVisibility( bool visible )
	{
		Instance.QtDriveHud.Enabled = visible;
	}
}
