using Sandbox;
using System;
using System.Linq;

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
	public Jettison Jettison { get; set; }
	[Property, Category( "Equipment" )]
	public Thruster MainThrusters { get; set; }
	[Property, Category( "Equipment" )]
	public Thruster Retrorockets { get; set; }

	private void FindEquipmentInChildren()
	{
		if ( !Hull.IsValid() ) Hull = Components.GetInDescendantsOrSelf<Health>();
		if ( !Shield.IsValid() ) Shield = Components.GetInDescendantsOrSelf<Shield>( true );
		if ( !Fuel.IsValid() ) Fuel = Components.GetInDescendantsOrSelf<FuelTank>();
		if ( !Grapple.IsValid() ) Grapple = Components.GetInDescendantsOrSelf<GrappleBeam>( true );
		if ( !Stabilizer.IsValid() ) Stabilizer = Components.GetInDescendantsOrSelf<Stabilizer>( true );
		if ( !Cargo.IsValid() ) Cargo = Components.GetInDescendantsOrSelf<CargoHold>();
		if ( !Jettison.IsValid() ) Jettison = Components.GetInDescendantsOrSelf<Jettison>( true );
		if ( !MainThrusters.IsValid() ) MainThrusters = Components
				.GetAll<Thruster>( FindMode.EnabledInSelfAndDescendants )
				.FirstOrDefault( t => !t.Retrorocket );
		if ( !Retrorockets.IsValid() ) Retrorockets = Components
				.GetAll<Thruster>( FindMode.EnabledInSelfAndDescendants )
				.FirstOrDefault( t => t.Retrorocket );
	}

	private void ApplyAllUpgrades()
	{
		var allUpgrades = ResourceLibrary.GetAll<Upgrade>();
		foreach( var upgradeResName in Career.Active.Upgrades )
		{
			var upgrade = allUpgrades.FirstOrDefault( u => u.ResourceName == upgradeResName );
			if ( upgrade is null )
			{
				Log.Error( $"Cannot find Upgrade by resource name: {upgradeResName}" );
				continue;
			}
			upgrade.OnApplyUpgrade( this );
		}
	}

	[ActionGraphNode("ship.equipment.cargo.capacity.add")]
	[Title("Add Cargo Capacity"), Group("Ship/Cargo")]
	public static void AddCargoCapacity( int slots )
	{
		var ship = GetCurrent();
		if ( slots < 0 || ship is null )
			return;

		ship.Cargo.MaxItems += slots;
	}

	[ActionGraphNode( "ship.equipment.jettison.add" )]
	[Title( "Add Jettison" ), Group( "Ship/Cargo" )]
	public static void AddJettison()
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		ship.Jettison.Enabled = true;
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

	[ActionGraphNode( "ship.equipment.thrusters.main.power.add")]
	[Title( "Add Main Thruster Power"), Group( "Ship/Thrusters")]
	public static void AddMainThrusterPower( float power )
	{
		var ship = GetCurrent();
		if ( power < 0 || ship is null )
			return;

		ship.MainThrusters.Power += power;
	}

	[ActionGraphNode( "ship.equipment.thrusters.retro.power.add")]
	[Title( "Add Retrorocket Power" ), Group( "Ship/Thrusters")]
	public static void AddRetrorocketPower( float power )
	{
		var ship = GetCurrent();
		if ( power < 0 || ship is null )
			return;

		ship.Retrorockets.Power += power;
	}

	[ActionGraphNode( "ship.equipment.stabilizer.add" )]
	[Title( "Add Stabilizer" ), Group( "Ship/Thrusters" )]
	public static void AddStabilizer()
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		ship.Stabilizer.Enabled = true;
	}

	[ActionGraphNode( "ship.equipment.thrusters.turn.speed.add" )]
	[Title( "Add Turn Speed" ), Group( "Ship/Thrusters" )]
	public static void AddTurnSpeed( float addSpeed )
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		ship.TurnSpeed += addSpeed;
	}

	[ActionGraphNode( "ship.equipment.shield.add" )]
	[Title( "Add Shield" ), Group( "Ship/Shield" )]
	public static void AddShield( float addHealth, float offsetDelay, float offsetRegenRate )
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		ship.Shield.Enabled = true;
		ship.Shield.MaxHealth += Math.Max( 0f, addHealth );
		ship.Shield.CurrentHealth += Math.Max( 0f, addHealth );
		ship.Shield.RegenDelay += offsetDelay;
		ship.Shield.RegenRate += offsetRegenRate;
	}
}
