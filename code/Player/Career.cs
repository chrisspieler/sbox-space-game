using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public class Career
{
	[ConVar("career_respawn_fee")]
	public static int RespawnFee { get; set; } = 100_000;

	public static Career Active { get; set; }

	public int Money
	{
		get => _money;
		set
		{
			_money = Math.Max( value, 0 );
		}
	}
	private int _money;

	public List<Upgrade> Upgrades { get; set; } = new();

	public IEnumerable<Upgrade> GetAvailableUpgrades()
	{
		return ResourceLibrary.GetAll<Upgrade>().Where( IsUpgradeAvailable );
	}

	public static bool IsUpgradeAvailable( Upgrade upgrade )
	{
		return !HasUpgrade( upgrade ) 
			&& (upgrade.PrerequisiteUpgrade is null || HasUpgrade( upgrade.PrerequisiteUpgrade ) );
	}

	[ConCmd("career_status")]
	public static void PrintStatus()
	{
		Log.Info( $"Active Career: {Active is not null}" );
		Log.Info( $"Career Credits: {Active?.Money ?? 0}" );
	}

	[ConCmd("add_money")]
	public static void AddMoney( int money )
	{
		if ( Active is null || money < 0 ) 
			return;

		Active.Money += money;
	}

	[ConCmd("remove_money")]
	public static void RemoveMoney( int money )
	{
		if ( Active is null || money < 0 )
			return;

		Active.Money -= money;
		Active.Money = Math.Max( 0, Active.Money );
	}

	public static bool HasMoney() => HasMoney( 1 );

	public static bool HasMoney( int money )
	{
		if ( Active is null )
			return false;

		return Active.Money >= money;
	}

	public static void AddUpgrade( Upgrade upgrade )
	{
		if ( HasUpgrade( upgrade ) )
			return;

		// Add all prerequisite upgrades recursively.
		if ( !HasUpgrade( upgrade.PrerequisiteUpgrade ) )
		{
			AddUpgrade( upgrade.PrerequisiteUpgrade );
		}

		Active.Upgrades.Add( upgrade );
		var ship = ShipController.GetCurrent();
		if ( ship is not null )
		{
			upgrade?.OnApplyUpgrade( ship );
		}
	}

	public static bool HasUpgrade( Upgrade upgrade )
	{
		return upgrade is null || Active.Upgrades.Contains( upgrade );
	}
}
