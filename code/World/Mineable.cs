using Sandbox;
using System;

public sealed class Mineable : Component
{
	[ConVar( "mining_loot_scale" )]
	public static float GlobalLootScale { get; set; } = 0.4f;
	[ConVar( "mining_damage_alert_radius" )]
	public static float DamageAlertRadius { get; set; } = 350f;

	[Property] public GameObject BreakEffect { get; set; }
	[Property] public Health Health { get; set; }
	[Property] public Asteroid Asteroid { get; set; }

	[Property] public bool ShrinkOnDamage { get; set; } = true;
	[Property, ShowIf(nameof(ShrinkOnDamage), true)] 
	public float ShrinkRatio { get; set; } = 0.3f;

	private Vector3? _startingScale;

	protected override void OnStart()
	{
		Health ??= Components.Get<Health>();
		if ( Health.IsValid() )
		{
			Health.OnKilled += Break;
			Health.OnDamaged += TakeDamage;
		}
		Asteroid ??= Components.Get<Asteroid>();
	}

	public void TakeDamage( DamageInfo damage )
	{
		if ( _startingScale is null )
		{
			_startingScale = Transform.Scale;
		}
		if ( damage.Attacker.Tags.Has( "player" ) )
		{
			Drone.AlertNearby( Transform.Position, DamageAlertRadius );
		}
		// Figure out how much of the mineable has been chipped away.
		var damageRatio = 1f - Health.CurrentHealth / Health.MaxHealth;
		Transform.Scale = _startingScale.Value - ShrinkRatio * damageRatio;
	}

	public void Break( DamageInfo damage )
	{
		SpawnBreakLoot();
		SpawnBreakEffect( damage );
		GameObject.Destroy();
	}

	private void SpawnBreakEffect( DamageInfo damage )
	{
		if ( BreakEffect is null )
			return;

		var effectGo = BreakEffect.Clone( Transform.Position );
		effectGo.Transform.ClearInterpolation();
	}

	private void SpawnBreakLoot()
	{
		if ( Asteroid is null )
			return;

		var lootFloat = Random.Shared.Float( 0.7f, 1.2f );
		lootFloat *= Health.MaxHealth / 75f;
		lootFloat *= Asteroid.Data.LootScale * GlobalLootScale;
		var lootCount = lootFloat.CeilToInt();
		for ( int i = 0; i < lootCount; i++ )
		{
			var pickupData = Asteroid.Data.LootTable.Choose();
			var pickupGo = Pickup.Spawn( pickupData, Transform.Position );
			var rb = pickupGo.Components.Get<Rigidbody>();
			rb.Velocity = Vector3.Random * Random.Shared.Float( 1500f, 3000f );
			rb.AngularVelocity = Vector3.Random * Random.Shared.Float( 1f, 10f );
		}
	}
}
