using Sandbox;
using System;

public sealed class Shop : Component, Component.ITriggerListener
{
	[ConVar( "shop_eject_invincibility_time" )]
	public static float EjectionInvincibilitySeconds { get; set; } = 1f;

	[Property] public float RefuelAmountPerSecond { get; set; } = 3f;
	[Property] public float FuelCostPerUnit { get; set; } = 10f;
	[Property] public float RepairCostPerHitPoint { get; set; } = 0.5f;
	[Property] public float BaseRepairCost { get; set; } = 20f;
	[Property] public GameObject Ejector { get; set; }
	[Property] public Vector3 EjectionVelocity { get; set; } = Vector3.Forward * 50f;

	private bool _isRefueling;
	private TimeUntil _untilRefuelUnit;

	protected override void OnUpdate()
	{
		if ( !_isRefueling )
		{
			_untilRefuelUnit = 1f / RefuelAmountPerSecond;
		}
		_isRefueling = false;
	}

	public void SellItem( CargoItem item, ShipController ship )
	{
		if ( !ship.IsValid() || !ship.Cargo.IsValid() )
			return;

		if ( !ship.Cargo.RemoveItem( item ) )
			return;

		Career.AddMoney( item.Value );
	}

	public bool CanRefuel( ShipController ship )
	{
		return Career.HasMoney( (int)FuelCostPerUnit )
			&& ship.Fuel.IsValid()
			&& ship.Fuel.CurrentAmount < ship.Fuel.MaxCapacity;
	}

	public void TickRefuel( ShipController ship )
	{
		if ( !CanRefuel( ship ) )
			return;

		_isRefueling = true;
		if ( _untilRefuelUnit )
		{
			var desiredAmount = ship.Fuel.CurrentAmount + 1;
			ship.Fuel.CurrentAmount = Math.Min( ship.Fuel.MaxCapacity, desiredAmount );
			Career.RemoveMoney( (int)FuelCostPerUnit );
			_untilRefuelUnit = 1f / RefuelAmountPerSecond;
		}
	}

	public int GetRepairPrice( ShipController ship )
	{
		if ( !ship.Hull.IsValid() || ship.Hull.CurrentHealth >= ship.Hull.MaxHealth )
			return 0;

		var damage = ship.Hull.MaxHealth - ship.Hull.CurrentHealth;
		var total = BaseRepairCost + damage * RepairCostPerHitPoint;
		return (int)total;
	}

	public void Repair( ShipController ship )
	{
		var price = GetRepairPrice( ship );
		if ( !Career.HasMoney( price ) || !ship.Hull.IsValid() )
			return;

		var damage = ship.Hull.MaxHealth - ship.Hull.CurrentHealth;
		if ( damage == 0f ) 
			return;

		ship.Hull.CurrentHealth = ship.Hull.MaxHealth;
		Career.RemoveMoney( price );
	}

	public void OnTriggerEnter( Collider other )
	{
		if ( !other.IsPlayer() )
			return;

		ScreenManager.OpenShopPanel( this );
		var ship = ShipController.GetCurrent();
		ship.Rigidbody.LinearDamping = 5f;
		ship.Rigidbody.Velocity = ship.Rigidbody.Velocity.ClampLength( 1000f );
		ship.Enabled = false;
	}

	public void OnTriggerExit( Collider other ) { }

	public void EjectPlayer()
	{
		var ship = ShipController.GetCurrent();
		if ( Ejector is not null )
		{
			ship.Transform.Position = Ejector.Transform.Position.WithZ( 0f );
			ship.Transform.Rotation = Ejector.Transform.Rotation;
			ship.FacingDirection = Ejector.Transform.Rotation.Forward;
			ship.Rigidbody.Velocity = ship.Transform.Rotation * EjectionVelocity;
		}
		ship.Rigidbody.LinearDamping = 0f;
		ship.AddTemporaryInvincibility( EjectionInvincibilitySeconds );
		ship.Enabled = true;
	}

	public bool CanBuyUpgrade( ShipController ship, Upgrade upgrade )
	{
		return ship.IsValid()
			&& !Career.HasUpgrade( upgrade )
			&& Career.IsUpgradeAvailable( upgrade )
			&& Career.HasMoney( upgrade.Cost );
	}

	public void BuyUpgrade( ShipController ship, Upgrade upgrade )
	{
		if ( !CanBuyUpgrade( ship, upgrade ) )
			return;

		Career.RemoveMoney( upgrade.Cost );
		Career.AddUpgrade( upgrade );
	}
}
