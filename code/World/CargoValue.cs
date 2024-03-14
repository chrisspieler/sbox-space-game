public class CargoValue
{
	public CargoValue( CargoItem cargo, int currentValue )
	{
		Cargo = cargo;
		CurrentValue = currentValue;
	}

	public CargoItem Cargo { get; set; }
	public int CurrentValue { get; set; }
}
