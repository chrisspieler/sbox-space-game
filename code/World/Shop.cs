using Sandbox;
using System.Collections.Generic;

public sealed class Shop : Component, Component.ITriggerListener
{
	[ConVar( "shop_eject_invincibility_time" )]
	public static float EjectionInvincibilitySeconds { get; set; } = 1f;
	[Property] public GameObject Ejector { get; set; }
	[Property] public Vector3 EjectionVelocity { get; set; } = Vector3.Forward * 50f;

	private CargoValues _cargoValues;

	protected override void OnStart()
	{
		_cargoValues = Career.Active.GetCargoValues();
	}

	public IEnumerable<CargoValue> GetAllCargoValues() => _cargoValues.GetAllValues();

	public int GetValue( CargoItem cargo )
	{
		return _cargoValues.GetCargoValue( cargo );
	}

	public int GetValue( CargoItem cargo, int quantity )
	{
		return _cargoValues.GetCargoValue( cargo, quantity );
	}

	public void SellItem( CargoItem item, ShipController ship )
	{
		if ( !ship.IsValid() || !ship.Cargo.IsValid() )
			return;

		if ( !ship.Cargo.RemoveItem( item ) )
			return;

		Career.Active.AddMoney( GetValue( item ) );
		_cargoValues.OnCargoSold( item );
		Career.Active.UpdateCargoValues( _cargoValues );
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !other.IsPlayer() )
			return;

		SaveManager.SaveActiveCareer();
		ScreenManager.SetHudEnabled( false );
		ScreenManager.SetCursorEnabled( false );
		ScreenManager.SetBeaconVisibility( false );
		ScreenManager.OpenShopPanel( this );
		var ship = ShipController.GetCurrent();
		ship.Rigidbody.LinearDamping = 5f;
		ship.Rigidbody.Velocity = ship.Rigidbody.Velocity.ClampLength( 1000f );
		ship.ActiveWeapon.ShouldFire = false;
		ship.Enabled = false;
	}

	public void OnTriggerExit( Collider other ) { }

	public void EjectPlayer()
	{
		SaveManager.SaveActiveCareer();
		ScreenManager.SetHudEnabled( true );
		ScreenManager.SetCursorEnabled( true );
		ScreenManager.SetBeaconVisibility( true );
		var ship = ShipController.GetCurrent();
		if ( !ship.IsValid() )
			return;
		if ( Ejector is not null )
		{
			ship.Transform.Position = Ejector.Transform.Position.WithZ( 0f );
			ship.Transform.Rotation = Ejector.Transform.Rotation;
			ship.FacingDirection = Ejector.Transform.Rotation.Forward;
			ship.Rigidbody.Velocity = ship.Transform.Rotation * EjectionVelocity;
		}
		// If the low fuel alarm is active and the player has left the store
		// without refueling, alert them to this silly mistake.
		ship.Fuel?.ClearAlarm();
		ship.Rigidbody.LinearDamping = 0f;
		ship.AddTemporaryInvincibility( EjectionInvincibilitySeconds );
		ship.Enabled = true;
	}

	public bool CanBuyEquipment( ShipController ship, Upgrade equipment )
	{
		return ship.IsValid()
			&& !Career.Active.HasEquipment( equipment )
			&& Career.Active.IsEquipmentAvailable( equipment )
			&& Career.Active.HasMoney( equipment.Cost );
	}

	public void BuyEquipment( ShipController ship, Upgrade equipment )
	{
		if ( !CanBuyEquipment( ship, equipment ) )
			return;

		Career.Active.RemoveMoney( equipment.Cost );
		Career.Active.AddEquipment( equipment );
		SaveManager.SaveActiveCareer();
	}

	public bool CanBuyUpgrade( ShipController ship, RepeatableUpgrade upgrade )
	{
		return ship.IsValid()
			&& Career.Active.GetUpgradeLevel( upgrade ) < upgrade.MaxLevel
			&& Career.Active.IsUpgradeAvailable( upgrade )
			&& Career.Active.HasMoney( Career.Active.GetUpgradeCost( upgrade ) );
	}

	public void BuyUpgrade( ShipController ship, RepeatableUpgrade upgrade )
	{
		if ( !CanBuyUpgrade( ship, upgrade ) )
			return;

		var nextLevel = Career.Active.GetUpgradeLevel( upgrade ) + 1;
		if ( nextLevel > upgrade.MaxLevel )
			return;

		Career.Active.RemoveMoney( Career.Active.GetUpgradeCost( upgrade ) );
		Career.Active.SetUpgradeLevel( upgrade, nextLevel );
		SaveManager.SaveActiveCareer();
	}
}
