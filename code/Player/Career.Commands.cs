﻿using Sandbox;
using System.Linq;

public partial class Career
{
	[ConCmd( "career_status" )]
	public static void PrintStatus()
	{
		Log.Info( $"Active Career: {Active is not null}" );
		Log.Info( $"Career Credits: {Active?.Money ?? 0}" );
		Log.Info( $"Upgrades" );
		Log.Info( $"---" );
		var allUpgrades = ResourceLibrary.GetAll<Upgrade>();
		foreach ( var upgradeResName in Active.Upgrades )
		{
			var upgrade = allUpgrades.FirstOrDefault( u => u.ResourceName == upgradeResName );
			Log.Info( $"\t{upgradeResName}: {upgrade?.Name ?? "Missing Upgrade"}" );
		}
	}

	[ConCmd( "add_money" ), Cheat]
	public static void AddMoneyCommand( int money )
	{
		Active?.AddMoney( money );
	}

	[ConCmd( "remove_money" ), Cheat]
	public static void RemoveMoneyCommmand( int money )
	{
		Active?.RemoveMoney( money );
	}

	[ConCmd( "add_all_upgrades"), Cheat]
	public static void AddAllUpgradesCommand()
	{
		Active?.AddAllUpgrades();
	}

	public void AddAllUpgrades()
	{
		var allEquipment = ResourceLibrary.GetAll<Upgrade>();
		foreach( var equipment in allEquipment )
		{
			AddEquipment( equipment );
		}
		var allUpgrades = ResourceLibrary.GetAll<RepeatableUpgrade>();
		foreach( var upgrade in allUpgrades )
		{
			SetUpgradeLevel( upgrade, upgrade.MaxLevel );
		}
	}
}
