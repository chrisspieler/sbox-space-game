using Sandbox;

public sealed class FuelTank : Component
{
	[ConVar("fuel_alarm_threshold")]
	public static float LowFuelAlarmThreshold { get; set; } = 0.25f;
	[Property] public float MaxCapacity { get; set; } = 10f;
	[Property] public float CurrentAmount { get; set; }
	[Property] public SoundEvent LowFuelAlarmSound { get; set; }

	private bool _hasAlarmed { get; set; }

	public void ClearAlarm() => _hasAlarmed = false;
	public bool HasFuel( float amount ) => ShipController.GodMode || CurrentAmount >= amount;

	protected override void OnStart()
	{
		CurrentAmount = MaxCapacity;
	}

	protected override void OnUpdate()
	{
		if ( _hasAlarmed )
		{
			if ( CurrentAmount > MaxCapacity * LowFuelAlarmThreshold )
			{
				ClearAlarm();
			}
			else
			{
				return;
			}
		}
		if ( CurrentAmount < MaxCapacity * LowFuelAlarmThreshold )
		{
			ScreenManager.ShowTextPanel( "LOW FUEL", Transform.Position + Transform.Rotation.Backward * 100f, true );
			Sound.Play( LowFuelAlarmSound, Transform.Position );
			_hasAlarmed = true;
		}
	}

	public bool TryBurnFuel( float amount )
	{
		if ( !HasFuel( amount ) || amount < 0f )
			return false;

		CurrentAmount -= ShipController.GodMode ? 0f : amount;
		return true;
	}

}
