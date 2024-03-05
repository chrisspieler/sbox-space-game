using Sandbox;
using System;

public sealed class Mineable : Component
{
	[Property] public GameObject FractureEffect { get; set; }
	[Property] public Health Health { get; set; }
	[Property] public LootTable FractureLoot { get; set; }
	[Property] public SoundEvent FractureSound { get; set; }

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
		// TODO: Shrink as damage is taken.
	}

	public void Fracture()
	{
		FractureEffect?.Clone( Transform.World.WithScale( 1f ) );
		// TODO: Spawn smaller versions of this object.
		if ( FractureLoot is not null )
		{
			var lootFloat = Transform.Scale.x * Random.Shared.Float( 0.7f, 1.2f ) * 0.7f;
			var lootCount = lootFloat.CeilToInt();
			for ( int i = 0; i < lootCount; i++ )
			{
				var pickupGo = Pickup.Spawn( FractureLoot.Choose(), Transform.Position );
				var rb = pickupGo.Components.Get<Rigidbody>();
				rb.Velocity = Vector3.Random * Random.Shared.Float( 1500f, 3000f );
				rb.AngularVelocity = Vector3.Random * Random.Shared.Float( 1f, 10f );
			}
		}
		if ( FractureSound is not null )
		{
			Sound.Play( FractureSound, Transform.Position );
		}
		GameObject.Destroy();
	}
}
