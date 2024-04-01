using Sandbox;
using System;

public partial class ControlStyleChoicePanel : PanelComponent
{
	[Property] public Action OnClose { get; set; }

	public static bool HasChosenTankControls { get; set; } = false;

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( UseTankControls );

	private string TapStyleButtonStyle => UseTankControls ? "" : "selected";
	private string TankStyleButtonStyle => UseTankControls ? "selected" : "";

	private bool UseTankControls { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		UseTankControls = ShipController.DefaultToTankControls;
	}

	private void Confirm()
	{
		ShipController.DefaultToTankControls = UseTankControls;
		HasChosenTankControls = true;
		Settings.SaveToDisk();
		Enabled = false;
		OnClose?.Invoke();
	}
}
