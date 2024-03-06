using Sandbox;
using System;

public sealed class Mineable : Component
{
	[ConVar( "mining_loot_scale" )]
	public static float LootScale { get; set; } = 0.4f;

	[Property] public GameObject FractureEffect { get; set; }
	[Property] public Health Health { get; set; }
	[Property] public LootTable FractureLoot { get; set; }

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
		// TODO: Shrink as damage is taken.
	}

	public void Fracture( DamageInfo damage )
	{
		SpawnFracturePieces();
		SpawnFractureLoot();
		SpawnFractureEffect( damage );
		GameObject.Destroy();
	}

	private void SpawnFractureEffect( DamageInfo damage )
	{
		var position = damage.Position.ToAbsolutePosition();
		var fractureGo = FractureEffect?.Clone();
		fractureGo.Transform.Position = position;
		var mover = fractureGo.Components.Create<MoveOnSpawn>();
		mover.AbsolutePosition = position;
	}

	private void SpawnFracturePieces()
	{
		// TODO: Spawn smaller versions of this object.
	}

	private void SpawnFractureLoot()
	{
		if ( FractureLoot is null )
			return;

		var lootFloat = Transform.Scale.x * Random.Shared.Float( 0.7f, 1.2f ) * LootScale;
		lootFloat *= Health.MaxHealth / 100f;
		var lootCount = lootFloat.CeilToInt();
		for ( int i = 0; i < lootCount; i++ )
		{
			var pickupGo = Pickup.Spawn( FractureLoot.Choose(), Transform.Position );
			var rb = pickupGo.Components.Get<Rigidbody>();
			rb.Velocity = Vector3.Random * Random.Shared.Float( 1500f, 3000f );
			rb.AngularVelocity = Vector3.Random * Random.Shared.Float( 1f, 10f );
		}
	}
}
