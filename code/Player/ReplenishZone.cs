using Sandbox;

public sealed class ReplenishZone : Component, Component.ITriggerListener
{
	[Property] public bool Refuel { get; set; } = true;
	[Property] public bool Repair { get; set; } = true;

	public void OnTriggerEnter( Collider other )
	{
		if ( !other.IsPlayer() )
			return;

		var ship = ShipController.GetCurrent();
		if ( ship is null )
			return;

		if ( Refuel && ship.Fuel?.Enabled == true && ship.Fuel.CurrentAmount < ship.Fuel.MaxCapacity )
		{
			ship.Fuel.CurrentAmount = ship.Fuel.MaxCapacity;
			ScreenManager.ShowTextPanel( "FUEL REPLENISHED", ship.Transform.Position, false );
		}
		if ( Repair && ship.Hull?.Enabled == true && ship.Hull.CurrentHealth < ship.Hull.MaxHealth )
		{
			ship.Hull.CurrentHealth = ship.Hull.MaxHealth;
			ScreenManager.ShowTextPanel( "HULL REPAIRED", ship.Transform.Position + Vector3.Backward * 100f, false );
		}
	}
}
