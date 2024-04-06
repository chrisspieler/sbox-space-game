using Sandbox;
using System.Collections.Generic;
using System.Linq;

public partial class Career
{
	public List<string> Upgrades { get; set; } = new();
	public List<RepeatableUpgradeState> RepeatableUpgrades { get; set; } = new();

	public IEnumerable<Upgrade> GetAvailableEquipment()
	{
		return ResourceLibrary.GetAll<Upgrade>().Where( IsEquipmentAvailable );
	}

	public bool IsEquipmentAvailable( Upgrade upgrade )
	{
		return !HasEquipment( upgrade )
			&& HasPrerequisites( upgrade ) && (Upgrade.ShowAllEquipment || !upgrade.Hidden);
	}

	public void AddEquipment( Upgrade upgrade )
	{
		if ( upgrade is null || HasEquipment( upgrade ) )
			return;

		// Add all prerequisite upgrades recursively.
		if ( !HasEquipment( upgrade.PrerequisiteUpgrade ) )
		{
			AddEquipment( upgrade.PrerequisiteUpgrade );
		}

		Upgrades.Add( upgrade.ResourceName );
		var ship = ShipController.GetCurrent();
		if ( ship.IsValid() )
		{
			upgrade.OnApplyUpgrade?.Invoke( ship );
		}
	}

	public bool HasEquipment( Upgrade upgrade )
	{
		return upgrade is null || Upgrades.Contains( upgrade.ResourceName );
	}

	private bool HasPrerequisites( Upgrade upgrade )
	{
		return upgrade.PrerequisiteUpgrade is null || HasEquipment( upgrade.PrerequisiteUpgrade );
	}

	private RepeatableUpgradeState GetUpgrade( RepeatableUpgrade upgrade )
	{
		if ( upgrade is null )
			return new RepeatableUpgradeState();

		return RepeatableUpgrades.FirstOrDefault( r => r.Name == upgrade.ResourceName );
	}

	private void SetUpgrade( RepeatableUpgrade upgrade, int level )
	{
		if ( upgrade is null )
			return;

		RepeatableUpgrades.Remove( GetUpgrade( upgrade ) );
		var state = new RepeatableUpgradeState()
		{
			Name = upgrade.ResourceName,
			Level = level
		};
		RepeatableUpgrades.Add( state );
	}

	public bool HasUpgrade( RepeatableUpgrade upgrade )
		=> upgrade is null || RepeatableUpgrades.Any( r => r.Name == upgrade.ResourceName );

	public bool HasPrerequisites( RepeatableUpgrade upgrade )
	{
		foreach( var prereq in upgrade.EquipmentPrereqs )
		{
			if ( !HasEquipment( prereq ) )
				return false;
		}
		return true;
	}

	public bool IsUpgradeAvailable( RepeatableUpgrade upgrade )
	{
		return HasPrerequisites( upgrade );
	}

	public IEnumerable<RepeatableUpgrade> GetAvailableUpgrades()
	{
		return ResourceLibrary.GetAll<RepeatableUpgrade>().Where( IsUpgradeAvailable );
	}

	public void RemoveUpgrade( RepeatableUpgrade upgrade )
	{
		RepeatableUpgrades.Remove( GetUpgrade( upgrade ) );
	}

	public int GetUpgradeLevel( RepeatableUpgrade upgrade )
	{
		return GetUpgrade( upgrade ).Level;
	}

	public void SetUpgradeLevel( RepeatableUpgrade upgrade, int level )
	{
		if ( upgrade is null )
			return;

		var currentLevel = GetUpgradeLevel( upgrade );
		// Rolling back upgrades is unsupported.
		if ( currentLevel >= level )
			return;

		foreach ( var prereq in upgrade.EquipmentPrereqs )
		{
			AddEquipment( prereq );
		}
		SetUpgrade( upgrade, level );

		for ( int i = currentLevel; i < level; i++ )
		{
			upgrade.OnIncreaseLevel?.Invoke( i );
		}
	}

	public int GetUpgradeCost( RepeatableUpgrade upgrade )
	{
		if ( upgrade?.GetCost is null )
			return 0;

		var currentLevel = GetUpgradeLevel( upgrade );
		return upgrade.GetCost( currentLevel );
	}
}
