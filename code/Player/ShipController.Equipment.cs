using Sandbox;
using System.Collections.Generic;

public partial class ShipController
{
	[Property, Category( "Equipment" )]
	public Health Hull 
	{
		get => _hull;
		set
		{
			var hullHasChanged = _hull != value;
			_hull = value;
			if ( GameManager.IsPlaying && hullHasChanged )
			{
				_hull.IsInvincible = IsInvincible;
				_hull.OnKilled += Explode;
			}
		}
	}
	private Health _hull;
	[Property, Category( "Equipment" )]
	public Shield Shield { get; set; }
	[Property, Category( "Equipment" )]
	public FuelTank Fuel { get; set; }
	[Property, Category( "Equipment" )]
	public GrappleBeam Grapple { get; set; }
	[Property, Category( "Equipment" )]
	public Stabilizer Stabilizer { get; set; }
	[Property, Category( "Equipment" )]
	public CargoHold Cargo { get; set; }
	[Property, Category( "Equipment" )]
	public List<Thruster> Thrusters { get; set; } = new();

	private void FindEquipmentInChildren()
	{
		if ( !Hull.IsValid() ) Hull = Components.GetInDescendantsOrSelf<Health>();
		if ( !Shield.IsValid() ) Shield = Components.GetInDescendantsOrSelf<Shield>();
		if ( !Fuel.IsValid() ) Fuel = Components.GetInDescendantsOrSelf<FuelTank>();
		if ( !Grapple.IsValid() ) Grapple = Components.GetInDescendantsOrSelf<GrappleBeam>();
		if ( !Stabilizer.IsValid() ) Stabilizer = Components.GetInDescendantsOrSelf<Stabilizer>();
		if ( !Cargo.IsValid() ) Cargo = Components.GetInDescendantsOrSelf<CargoHold>();
		foreach( var thruster in Components.GetAll<Thruster>( FindMode.EnabledInSelfAndDescendants ) )
		{
			if ( !Thrusters.Contains( thruster ) )
			{
				Thrusters.Add( thruster );
			}
		}
	}
}
