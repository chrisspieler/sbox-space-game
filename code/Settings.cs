using System.Text.Json.Serialization;

using Sandbox;

public class Settings
{
	[JsonIgnore]
	public const string FileName = "settings.json";

	public float BloomIntensity { get; set; }
	public float ScreenShakeScale { get; set; }
	public float LaserBrightnessScale { get; set; }
	public float FogThicknessScale { get; set; }
	public float AsteroidHighlightScale { get; set; }
	public bool HasChosenTankControls { get; set; }
	public bool UseTankControls { get; set; }
	public float TankControlsTurnSpeed { get; set; }

	public static void LoadAndApply()
	{
		Settings settings = null;
		if ( FileSystem.Data.FileExists( FileName ) )
		{
			settings = FileSystem.Data.ReadJson<Settings>( FileName );
		}
		if ( settings is null )
		{
			Log.Info( $"Unable to load settings, using defaults." );
			settings ??= GetCurrentSettings();
			SaveToDisk();
		}
		ApplySettings( settings );
	}

	private static Settings GetCurrentSettings()
	{
		return new Settings
		{
			ScreenShakeScale = ScreenShake.ScreenShakeScale,
			LaserBrightnessScale = LaserBeam.BrightnessScale,
			FogThicknessScale = FogController.IntensityScale,
			BloomIntensity = PostProcessingController.BloomIntensity,
			AsteroidHighlightScale = AsteroidHighlight.GlobalStrength,
			UseTankControls = ShipController.DefaultToTankControls,
			TankControlsTurnSpeed = ShipController.TankControlsTurnSpeedFactor,
			HasChosenTankControls = ControlStyleChoicePanel.HasChosenTankControls
		};
	}

	private static void ApplySettings( Settings settings )
	{
		ScreenShake.ScreenShakeScale = settings.ScreenShakeScale;
		LaserBeam.BrightnessScale = settings.LaserBrightnessScale;
		FogController.IntensityScale = settings.FogThicknessScale;
		PostProcessingController.BloomIntensity = settings.BloomIntensity;
		AsteroidHighlight.GlobalStrength = settings.AsteroidHighlightScale;
		ShipController.DefaultToTankControls = settings.UseTankControls;
		ShipController.TankControlsTurnSpeedFactor = settings.TankControlsTurnSpeed;
		ControlStyleChoicePanel.HasChosenTankControls = settings.HasChosenTankControls;
	}

	public static void SaveToDisk()
	{
		var settings = GetCurrentSettings();
		FileSystem.Data.WriteJson( FileName, settings );
	}
}
