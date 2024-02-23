using Sandbox;

public sealed class ScreenManager : Component
{
	public static ScreenManager Instance { get; private set; }

	[Property] public PanelComponent RestartPanel { get; set; }
	[Property] public PanelComponent HudPanel { get; set; }
	[Property] public PanelComponent CursorPanel { get; set; }
	[Property] public PanelComponent HoveredSelectionPanel { get; set; }

	protected override void OnStart()
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
}
