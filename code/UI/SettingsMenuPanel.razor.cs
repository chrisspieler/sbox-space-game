using Sandbox;

public partial class SettingsMenuPanel : PanelComponent
{
	[Property] public PanelComponent ReturnMenu { get; set; }
	[Property] public PanelComponent ControlStyleMenu { get; set; }

	protected override int BuildHash() => System.HashCode.Combine( ScreenShake.ScreenShakeScale, LaserBeam.BrightnessScale, FogController.IntensityScale, PostProcessingController.BloomIntensity, AsteroidHighlight.GlobalStrength, _showModal );

	private string ModalClass => _showModal ? "" : "hidden";
	private bool _showModal = false;

	private bool _clickedBack = false;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Input.EscapePressed && !ControlStyleMenu.Enabled )
		{
			Input.EscapePressed = false;
			_showModal = false;
			Back();
		}
	}

	private void SetScreenShakeScale( float value )
	{
		ScreenShake.ScreenShakeScale = value;
	}

	private void SetLaserBrightnessScale( float value )
	{
		LaserBeam.BrightnessScale = value;
	}

	private void SetFogIntensityScale( float value )
	{
		FogController.IntensityScale = value;
	}

	private void SetBloomIntensityScale( float value )
	{
		PostProcessingController.BloomIntensity = value;
	}

	private void SetAsteroidHighlightScale( float value )
	{
		AsteroidHighlight.GlobalStrength = value;
	}

	private void SetTankControlsTurnSpeed( float value )
	{
		ShipController.TankControlsTurnSpeedFactor = value;
	}

	private void WipeSaveData()
	{
		SaveManager.DeleteSaveFile( SaveManager.GetSaveSlotFileName( 0 ) );
		_showModal = false;
	}

	private void Back()
	{
		if ( _clickedBack )
			return;

		SetClass( "hidden", true );
		Settings.SaveToDisk();
		ReturnMenu.Enabled = true;
		_ = Task.DelayRealtimeSeconds( 0.15f ).ContinueWith( _ => { Enabled = false; } );
	}
}
