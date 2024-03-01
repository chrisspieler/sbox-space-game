using Sandbox;
using System.Collections.Generic;

public partial class ShipController
{
	[Property, Category( "Equipment" )]
	public Health Hull 
	{
		get => _hull;
		set
		{
			var hullHasChanged = _hull != value;
			_hull = value;
			if ( GameManager.IsPlaying && hullHasChanged )
			{
				_hull.IsInvincible = IsInvincible;
				_hull.OnKilled += Explode;
			}
		}
	}
	private Health _hull;
	[Property, Category( "Equipment" )]
	public Shield Shield { get; set; }
	[Property, Category( "Equipment" )]
	public FuelTank Fuel { get; set; }
	[Property, Category( "Equipment" )]
	public GrappleBeam Grapple { get; set; }
	[Property, Category( "Equipment" )]
	public Stabilizer Stabilizer { get; set; }
	[Property, Category( "Equipment" )]
	public CargoHold Cargo { get; set; }
	[Property, Category( "Equipment" )]
	public List<Thruster> Thrusters { get; set; } = new();

	private void FindEquipmentInChildren()
	{
		if ( !Hull.IsValid() ) Hull = Components.GetInDescendantsOrSelf<Health>();
		if ( !Shield.IsValid() ) Shield = Components.GetInDescendantsOrSelf<Shield>();
		if ( !Fuel.IsValid() ) Fuel = Components.GetInDescendantsOrSelf<FuelTank>();
		if ( !Grapple.IsValid() ) Grapple = Components.GetInDescendantsOrSelf<GrappleBeam>();
		if ( !Stabilizer.IsValid() ) Stabilizer = Components.GetInDescendantsOrSelf<Stabilizer>();
		if ( !Cargo.IsValid() ) Cargo = Components.GetInDescendantsOrSelf<CargoHold>();
		foreach( var thruster in Components.GetAll<Thruster>( FindMode.EnabledInSelfAndDescendants ) )
		{
			if ( !Thrusters.Contains( thruster ) )
			{
				Thrusters.Add( thruster );
			}
		}
	}

	private void ApplyAllUpgrades()
	{
		foreach( var upgrade in Career.Active.Upgrades )
		{
			upgrade.OnApplyUpgrade( this );
		}
	}

	[ActionGraphNode("ship.equipment.cargo.capacity.add")]
	[Title("Add Cargo Capacity"), Group("Ship/Equipment")]
	public static void AddCargoCapacity( int slots )
	{
		var ship = GetCurrent();
		if ( slots < 0 || ship is null )
			return;

		ship.Cargo.MaxItems += slots;
	}

	[ActionGraphNode( "ship.equipment.fuel.capacity.add" )]
	[Title( "Add Fuel Capacity" ), Group( "Ship/Fuel" )]
	public static void AddFuelCapacity( int fuel )
	{
		var ship = GetCurrent();
		if ( fuel < 0 || ship is null )
			return;

		ship.Fuel.MaxCapacity += fuel;
		ship.Fuel.CurrentAmount += fuel;
	}
}
