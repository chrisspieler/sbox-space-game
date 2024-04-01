using Sandbox;
using System.Linq;

public partial class HudPanel : PanelComponent
{
	[Property] public ShipController Ship { get; set; }
	[Property, Category( "Style" )] public Color ShieldColor { get; set; } = Color.Cyan;
	[Property, Category( "Style" )] public Color HullColor { get; set; } = Color.Red;
	[Property, Category( "Style" )] public Color FuelColor { get; set; } = Color.Yellow;

	private string _shieldBarStyle => GetShieldPercent() > 0f
		? ""
		: "hidden";


	protected override int BuildHash() => System.HashCode.Combine( ShieldColor, HullColor, FuelColor, Ship?.Shield?.CurrentHealth == 0f, GetCargoCapacity(), GetCargoCount() );

	protected override void OnStart()
	{
		var ship = Scene.GetAllComponents<ShipController>().FirstOrDefault();
		UpdateShip( ship );
	}

	public void UpdateShip( ShipController ship )
	{
		Ship = ship;
	}

	private float GetShieldPercent()
	{
		if ( !Ship.IsValid() || !Ship.Shield.IsValid() || !Ship.Shield.Enabled )
			return 0f;

		return (Ship.Shield.CurrentHealth / Ship.Shield.MaxHealth) * 100f;
	}

	private float GetFuelPercent()
	{
		if ( !Ship.IsValid() || !Ship.Fuel.IsValid() )
			return 0f;

		return (Ship.Fuel.CurrentAmount / Ship.Fuel.MaxCapacity) * 100f;
	}

	private float GetHullPercent()
	{
		if ( !Ship.IsValid() || !Ship.Hull.IsValid() )
			return 0f;

		return (Ship.Hull.CurrentHealth / Ship.Hull.MaxHealth) * 100f;
	}

	private int GetCargoCount()
	{
		if ( !Ship.IsValid() || !Ship.Cargo.IsValid() )
			return 0;

		return Ship.Cargo.Items.Count;
	}

	private int GetCargoCapacity()
	{
		if ( !Ship.IsValid() || !Ship.Cargo.IsValid() )
			return 0;

		return Ship.Cargo.MaxItems;
	}
}
