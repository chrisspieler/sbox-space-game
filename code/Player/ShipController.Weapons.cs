using Sandbox;
using System.Collections.Generic;

public partial class ShipController
{
	[Property] public GameObject WeaponPointLeft { get; set; }
	[Property] public GameObject WeaponPointRight { get; set; }
	[Property] public Weapon ActiveWeapon { get; set; }
	[Property] public MiningLaser Laser { get; set; }

	private void FindWeaponsInChildren()
	{
		if ( !Laser.IsValid() ) Laser = Components.GetInDescendantsOrSelf<MiningLaser>( true );
	}
	
	[ActionGraphNode( "ship.weapons.laser.power.add" )]
	[Title( "Add Laser Power" ), Group( "Ship/Weapons/Laser" )]
	public static void AddLaserPower( float tickIntervalOffset, float tickDamageOffset )
	{
		var ship = GetCurrent();
		if ( ship is null )
			return;

		ship.Laser.TickDamage += tickDamageOffset;
		ship.Laser.TickInterval += tickIntervalOffset;
	}
}
