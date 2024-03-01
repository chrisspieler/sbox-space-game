using Sandbox;
using System;

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
		FractureEffect?.Clone( Transform.World.WithScale( 1f ) );
		// TODO: Spawn smaller versions of this object.
		// TODO: Drop loot per loot table.
		if ( FractureLoot is not null )
		{
			var lootFloat = Transform.Scale.x * Random.Shared.Float( 0.7f, 1.2f ) / 2f;
			var lootCount = lootFloat.CeilToInt();
			for ( int i = 0; i < lootCount; i++ )
			{
				Pickup.Spawn( FractureLoot, Transform.Position );
			}
		}
		GameObject.Destroy();
	}
}
