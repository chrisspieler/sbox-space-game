using System;

namespace Sandbox;

[GameResource("Ship Upgrade", "upgrade", "An upgrade that may be applied to the ship.", Icon = "upgrade")]
public class Upgrade : GameResource
{
	[ConVar( "upgrade_show_all" )]
	public static bool ShowAllUpgrades { get; set; }

	public string Name { get; set; }
	public string FlavorText { get; set; }
	public string FunctionText { get; set; }
	public int Cost { get; set; } = 50;
	public Texture Thumbnail { get; set; }
	public Upgrade PrerequisiteUpgrade { get; set; }
	public Action<ShipController> OnApplyUpgrade { get; set; }
	public bool Hidden { get; set; }
}
