using System;

namespace Sandbox;

[GameResource("Ship Equipment", "upgrade", "A piece of ship equipment.", Icon = "construction")]
public class Upgrade : GameResource
{
	[ConVar( "equipment_show_all" )]
	public static bool ShowAllEquipment { get; set; }

	public string Name { get; set; }
	public string FlavorText { get; set; }
	public string FunctionText { get; set; }
	public int Cost { get; set; } = 50;
	public Texture Thumbnail { get; set; }
	public Upgrade PrerequisiteUpgrade { get; set; }
	public Action<ShipController> OnApplyUpgrade { get; set; }
	public bool Hidden { get; set; }
}
