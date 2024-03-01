using Sandbox;
using System.Collections.Generic;

[GameResource("Scenario", "scenario", "The starting conditions of a career.", Icon = "flag" )]
public class Scenario : GameResource
{
	public string Name { get; set; }
	public string Description { get; set; }
	public int Money { get; set; } = 0;
	public List<Upgrade> Upgrades { get; set; }
	public bool Hidden { get; set; } = true;
}
