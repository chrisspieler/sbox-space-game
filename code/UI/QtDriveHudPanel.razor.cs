using Sandbox;

public partial class QtDriveHudPanel : PanelComponent
{
	protected override int BuildHash() => System.HashCode.Combine( GetFillPercent );

	private float GetFillPercent()
	{
		var ship = ShipController.GetCurrent();
		if ( ship is null || ship.QtDrive is null )
		{
			Enabled = false;
			return 0f;
		}
		return ship.QtDrive.CurrentCharge / ship.QtDrive.MaxCharge * 100f;
	}
}
