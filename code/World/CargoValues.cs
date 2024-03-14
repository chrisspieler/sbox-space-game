using System;
using System.Collections.Generic;
using System.Linq;

public class CargoValues
{
	private Dictionary<CargoItem, int> _currentCargoValues = new();
	private Dictionary<CargoItem, int> _ticksUntilNextIncrease = new();

	public CargoValues( IEnumerable<CargoValue> prices )
	{
		foreach( var item in prices )
		{
			_currentCargoValues.Add( item.Cargo, item.CurrentValue );
			_ticksUntilNextIncrease.Add( item.Cargo, item.Cargo.OtherSalesUntilValueIncrease );
		}
	}

	public IEnumerable<CargoValue> GetAllValues()
	{
		return _currentCargoValues.Select( v => new CargoValue( v.Key, v.Value ) );
	}

	public void OnCargoSold( CargoItem cargo )
	{
		if ( !_currentCargoValues.ContainsKey( cargo ) )
			return;

		// After cargo is sold, it sells for less...
		_currentCargoValues[cargo] = Math.Max( cargo.MinValue, _currentCargoValues[cargo] - cargo.ValueDecreaseOnSale);
		// ...but gradually regains its price as other cargo is sold.
		_ticksUntilNextIncrease[cargo] = cargo.OtherSalesUntilValueIncrease;
		// For all cargo except for that which was just sold...
		foreach( var otherCargo in _currentCargoValues.Keys.Where( c => c != cargo ) )
		{
			_ticksUntilNextIncrease[otherCargo] -= 1;
			// ...check whether it's time to increase the value of the other cargo.
			if ( _ticksUntilNextIncrease[otherCargo] <= 0 )
			{
				_ticksUntilNextIncrease[otherCargo] = otherCargo.OtherSalesUntilValueIncrease;
				_currentCargoValues[otherCargo] += otherCargo.ValueIncreaseAmount;
			}
		}
	}

	public bool HasCargo( CargoItem cargo )
	{
		return _currentCargoValues.ContainsKey( cargo );
	}

	public int GetCargoValue( CargoItem cargo )
	{
		if ( !HasCargo( cargo ) )
			return 0;

		return _currentCargoValues[cargo];
	}

	public int GetCargoValue( CargoItem cargo, int quantity )
	{
		if ( !HasCargo( cargo ) )
			return 0;

		var totalValue = 0;
		var currentValue = _currentCargoValues[cargo];
		for ( var i = 0; i < quantity; i++ )
		{
			totalValue += currentValue;
			currentValue = Math.Max( cargo.MinValue, currentValue - cargo.ValueDecreaseOnSale);
		}
		return totalValue;
	}
}
