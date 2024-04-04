using Sandbox;
using System.Collections.Generic;

[GameResource( "Repeatable Ship Upgrade", "repup", "An upgrade that may be repeatedly applied to the ship.", Icon = "upgrade" )]
public class RepeatableUpgrade : GameResource
{
	/// <summary>
	/// Given the current level, returns the number of credits that would
	/// be required to upgrade to the next level.
	/// </summary>
	public delegate int LevelCostCalculation( int currentLevel );
	/// <summary>
	/// Invoked after increasing the level of an upgrade. Given the new level,
	/// applies the effect of the upgrade in-game.
	/// </summary>
	public delegate void LevelIncreaseAction( int newLevel );

	public string Name { get; set; }
	public string FunctionText { get; set; }
	public Texture Thumbnail { get; set; }
	public List<Upgrade> EquipmentPrereqs { get; set; }
	public int MaxLevel { get; set; } = 15;
	public LevelCostCalculation GetCost { get; set; }
	public LevelIncreaseAction OnIncreaseLevel { get; set; }
}
