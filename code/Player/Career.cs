using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class Career
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

	public List<string> Upgrades { get; set; } = new();

	public float TotalPlayTime { get; set; }

	public string GetPlayTimeString()
	{
		var time = TimeSpan.FromSeconds( TotalPlayTime );
		return time.ToString( "hh\\:mm\\:ss" );
	}

	public IEnumerable<Upgrade> GetAvailableUpgrades()
	{
		return ResourceLibrary.GetAll<Upgrade>().Where( IsUpgradeAvailable );
	}

	public bool IsUpgradeAvailable( Upgrade upgrade )
	{
		return !HasUpgrade( upgrade ) 
			&& (upgrade.PrerequisiteUpgrade is null || HasUpgrade( upgrade.PrerequisiteUpgrade ) );
	}

	public bool HasMoney() => HasMoney( 1 );

	public bool HasMoney( int money ) => Money >= money;

	public void AddMoney( int money )
	{
		if ( money < 0 )
			return;

		Money += money;
	}
	public void RemoveMoney( int money )
	{
		if ( money < 0 )
			return;

		Money -= money;
		Money = Math.Max( 0, Money );
	}

	public void AddUpgrade( Upgrade upgrade )
	{
		if ( HasUpgrade( upgrade ) )
			return;

		// Add all prerequisite upgrades recursively.
		if ( !HasUpgrade( upgrade.PrerequisiteUpgrade ) )
		{
			AddUpgrade( upgrade.PrerequisiteUpgrade );
		}

		Upgrades.Add( upgrade.ResourceName );
		var ship = ShipController.GetCurrent();
		if ( ship is not null )
		{
			upgrade?.OnApplyUpgrade( ship );
		}
	}

	public bool HasUpgrade( Upgrade upgrade )
	{
		return upgrade is null || Upgrades.Contains( upgrade.ResourceName );
	}
}
