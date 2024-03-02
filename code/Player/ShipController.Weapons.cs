using Sandbox;
using System.Collections.Generic;

public partial class ShipController
{
	[Property] public List<Weapon> Lasers { get; set; } = new();

	private void FindWeaponsInChildren()
	{
		Lasers.AddRange( Components.GetAll<Weapon>( FindMode.EverythingInSelfAndDescendants ) );
	}

	[ActionGraphNode( "ship.weapons.laser.power.add" )]
	[Title( "Add Laser Power" ), Group( "Ship/Weapons/Laser" )]
	public static void AddLaserPower( float tickIntervalOffset, float tickDamageOffset )
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		foreach( var laser in ship.Lasers )
		{
			laser.TickInterval += tickIntervalOffset;
			laser.TickDamage += tickDamageOffset;
		}
	}
}
