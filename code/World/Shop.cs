using Sandbox;
using System;

public sealed class Shop : Component, Component.ITriggerListener
{
	[Property] public float RefuelAmountPerSecond { get; set; } = 3f;
	[Property] public float FuelCostPerUnit { get; set; } = 10f;
	[Property] public float RepairCostPerHitPoint { get; set; } = 0.5f;
	[Property] public float BaseRepairCost { get; set; } = 20f;



	private bool _isRefueling;
	private TimeUntil _untilRefuelUnit;
	private bool _isPlayerInRange;

	protected override void OnUpdate()
	{
		if ( !_isRefueling )
		{
			_untilRefuelUnit = 1f / RefuelAmountPerSecond;
		}
		_isRefueling = false;
		EnsurePlayerStillAlive();
		if ( _isPlayerInRange && Input.Pressed( "use" ) )
		{
			if ( ScreenManager.IsShopOpen() )
			{
				ScreenManager.CloseShopPanel();
			}
			else
			{
				ScreenManager.OpenShopPanel( this );
			}
		}
		if ( !_isPlayerInRange && ScreenManager.IsShopOpen() )
		{
			ScreenManager.CloseShopPanel();
		}
	}

	private void EnsurePlayerStillAlive()
	{
		var ship = ShipController.GetCurrent();
		if ( !ship.IsValid() || !ship.IsAlive )
		{
			_isPlayerInRange = false;
		}
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

		_isPlayerInRange = true;
		ScreenManager.SetNearbyShopIndicator( true );
	}

	public void OnTriggerExit( Collider other ) 
	{
		if ( !other.IsPlayer() )
			return;

		_isPlayerInRange = false;
		ScreenManager.SetNearbyShopIndicator( false );
	}
}
