using Sandbox;
using System.Collections.Generic;

[GameResource("Scenario", "scenario", "The starting conditions of a career.", Icon = "flag" )]
public class Scenario : GameResource
{
	[ConVar("scenario")]
	public static string DefaultScenario { get; set; } = string.Empty;

	public string Name { get; set; }
	public string Description { get; set; }
	public int Money { get; set; } = 0;
	public List<Upgrade> Upgrades { get; set; }
	public WorldMap World { get; set; }
	public bool Hidden { get; set; } = true;

	public Career ToCareer()
	{
		var career = new Career()
		{
			Money = Money
		};
		foreach ( var upgrade in Upgrades )
		{
			career.AddEquipment( upgrade );
		}
		career.World = World.ResourceName;
		return career;
	}
}
