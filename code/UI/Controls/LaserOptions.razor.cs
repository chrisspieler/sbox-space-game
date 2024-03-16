using Sandbox;
using Sandbox.UI;
using System;

public partial class LaserOptions : Panel
{
	protected override int BuildHash() => HashCode.Combine( Time.Now );

	private bool RainbowMode 
	{
		get => Career.Active.LaserStylePreference == Career.LaserStyle.Gradient;
		set
		{
			Career.Active.LaserStylePreference = value
				? Career.LaserStyle.Gradient
				: Career.LaserStyle.Tinted;
			ShipController.UpdateLaserColor();
		}
	}

	private Color GetPreviewColor()
	{
		if ( Career.Active.LaserStylePreference == Career.LaserStyle.Gradient )
		{
			return Career.Active.GetPreferredLaserGradient().GetColor();
		}
		else
		{
			return Career.Active.LaserTintPreference;
		}
	}

	private void SetHue( float newHue )
	{
		Career.Active.LaserTintPreference = new ColorHsv( newHue, 1f, 1f );
		ShipController.UpdateLaserColor();
	}

	private void SetRainbowMode( bool enabled )
	{
		RainbowMode = enabled;
	}
}
