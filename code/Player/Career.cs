using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class Career
{
	public struct SavedCargoValue
	{
		public string Name { get; set; }
		public int Value { get; set; }

		public CargoValue ToCargoValue()
		{
			var thisName = Name.ToLower();
			var cargo = ResourceLibrary.GetAll<CargoItem>()
				.FirstOrDefault( c => c.ResourceName.ToLower() == thisName );
			if ( cargo is null )
				return null;
			return new CargoValue( cargo, Value );
		}
	}

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
	public List<SavedCargoValue> ShopCargoValues { get; set; } = new();
	public string World { get; set; } = "default";

	public float TotalPlayTime { get; set; }

	public CargoValues GetCargoValues()
	{
		if ( !ShopCargoValues.Any() )
		{
			var defaultValues = ResourceLibrary.GetAll<CargoItem>()
				.Select( c => new CargoValue( c, c.BaseValue ) );
			UpdateCargoValues( new CargoValues( defaultValues ) );
		}
		var values = ShopCargoValues.Select( v => v.ToCargoValue() );
		return new CargoValues( values );
	}

	public void UpdateCargoValues( CargoValues newValues )
	{
		ShopCargoValues = newValues
			.GetAllValues()
			.Select( v => new SavedCargoValue() { Name = v.Cargo.ResourceName, Value = v.CurrentValue } )
			.ToList();
	}

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
			&& HasPrerequisites( upgrade ) && ( Upgrade.ShowAllUpgrades || !upgrade.Hidden );
	}

	private bool HasPrerequisites( Upgrade upgrade )
	{
		return upgrade.PrerequisiteUpgrade is null || HasUpgrade( upgrade.PrerequisiteUpgrade );
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
			upgrade?.OnApplyUpgrade?.Invoke( ship );
		}
	}

	public bool HasUpgrade( Upgrade upgrade )
	{
		return upgrade is null || Upgrades.Contains( upgrade.ResourceName );
	}
}
