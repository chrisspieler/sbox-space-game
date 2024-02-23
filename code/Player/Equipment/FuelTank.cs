using Sandbox;

public sealed class FuelTank : Component
{
	[Property] public float MaxCapacity { get; set; } = 10f;
	[Property] public float CurrentAmount { get; set; }

	protected override void OnStart()
	{
		CurrentAmount = MaxCapacity;
	}

	public bool HasFuel( float amount ) => CurrentAmount >= amount;

	public bool TryBurnFuel( float amount )
	{
		if ( !HasFuel( amount ) || amount < 0f )
			return false;

		CurrentAmount -= amount;
		return true;
	}

}
