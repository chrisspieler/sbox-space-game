using System;
using System.Linq;

namespace Sandbox.Debug
{
	public static class CargoCommands
	{
		[ConCmd("cargo_fill_random"), Cheat]
		public static void FillCargoHold()
		{
			var ship = ShipController.GetCurrent();
			if ( ship?.Cargo is null )
				return;

			var allCargo = ResourceLibrary.GetAll<CargoItem>().ToArray();
			var remainingCapacity = ship.Cargo.MaxItems - ship.Cargo.Count;
			for ( int i = 0; i < remainingCapacity; i++ )
			{
				var random = Random.Shared.FromArray( allCargo );
				ship.Cargo.TryAddItem( random );
			}
		}

		[ConCmd("cargo_fill"), Cheat]
		public static void FillCargoHold( string cargoName )
		{
			var ship = ShipController.GetCurrent();
			if ( ship?.Cargo is null )
				return;

			var cargo = ResourceLibrary.GetAll<CargoItem>()
				.FirstOrDefault( c => c.ResourceName.ToLower() == cargoName.ToLower() );
			if ( cargo is null )
			{
				Log.Info( $"Cargo item not found: {cargoName}" );
				return;
			}
			var remainingCapacity = ship.Cargo.MaxItems - ship.Cargo.Count;
			for ( int i = 0; i < remainingCapacity; i++ )
			{
				ship.Cargo.TryAddItem( cargo );
			}
		}
	}
}
