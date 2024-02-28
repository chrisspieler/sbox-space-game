using Sandbox;

public sealed class Mineable : Component
{
	[Property] public GameObject FractureEffect { get; set; }
	[Property] public Health Health { get; set; }
	// TODO: Replace this with a loot table GameResource.
	[Property] public CargoItem FractureLoot { get; set; }

	protected override void OnStart()
	{
		Health ??= Components.Get<Health>();
		if ( Health.IsValid() )
		{
			Health.OnKilled += Fracture;
			Health.OnDamaged += TakeDamage;
		}
	}

	public void TakeDamage( DamageInfo damage )
	{
		// TODO: Drop loot per loot table.
	}

	public void Fracture()
	{
		FractureEffect?.Clone( Transform.Position );
		// TODO: Spawn smaller versions of this object.
		// TODO: Drop loot per loot table.
		if ( FractureLoot is not null )
		{
			Pickup.Spawn( FractureLoot, Transform.Position );
		}
		GameObject.Destroy();
	}
}
