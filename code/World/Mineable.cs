using Sandbox;
using System;

public sealed class Mineable : Component
{
	[ConVar( "mining_loot_scale" )]
	public static float LootScale { get; set; } = 0.4f;

	[Property] public GameObject FractureEffect { get; set; }
	[Property] public Health Health { get; set; }
	[Property] public LootTable FractureLoot { get; set; }
	[Property] public GameObject FracturePiecePrefab { get; set; }
	[Property] public bool ShrinkOnDamage { get; set; } = true;
	[Property, ShowIf(nameof(ShrinkOnDamage), true)] 
	public float ShrinkRatio { get; set; } = 0.3f;

	private Vector3? _startingScale;

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
		if ( _startingScale is null )
		{
			_startingScale = Transform.Scale;
		}
		// Figure out how much of the mineable has been chipped away.
		var damageRatio = 1f - Health.CurrentHealth / Health.MaxHealth;
		Transform.Scale = _startingScale.Value - ShrinkRatio * damageRatio;
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
		FractureEffect?.Clone( damage.Position );
	}

	private void SpawnFracturePieces()
	{
		// Avoid spawning really tiny pieces that are difficult to hit.
		if ( Transform.Scale.x < 3f )
		{
			return;
		}	
		// Shed some scale when fracturing.
		var scale = Transform.Scale * 0.7f;
		var maxPieces = Math.Max( 2, (scale.x * 0.7f).FloorToInt() );
		var pieceCount = Random.Shared.Int( 2, maxPieces );
		for( int i = 0; i < pieceCount; i++ )
		{
			var pieceScale = scale / pieceCount;
			Log.Info( $"Spawning fragment with scale {pieceScale}" );
			SpawnFragment( pieceScale );
		}
	}

	private void SpawnFragment( Vector3 scale )
	{
		var bounds = GameObject.GetBounds();
		bounds.Maxs -= bounds.Size / 2f;
		bounds.Mins += bounds.Size / 2f;
		var pieceGo = FracturePiecePrefab.Clone( bounds.RandomPointInside.WithZ( 0f ) );
		// Stop the floater from interfering.
		if ( pieceGo.Components.TryGet<SpaceFloater>( out var floater ) )
		{
			floater.Enabled = false;
			floater.Destroy();
		}
		var randomPos = Random.Shared.VectorInSquare( Transform.Scale.Length / 2f );
		pieceGo.Transform.Position += new Vector3( randomPos.x, randomPos.y );
		pieceGo.Transform.Rotation = Rotation.Random;
		pieceGo.Transform.Scale = scale;
		if ( pieceGo.Components.TryGet<Rigidbody>( out var rb ) )
		{
			rb.PhysicsBody.Mass = 240f * scale.x;
			rb.Velocity = Vector2.Random.Normal * Random.Shared.Float( 25f, 50f );
			rb.AngularVelocity = Vector3.Random * Random.Shared.Float( 0.5f, 3f );
		}
		var mineable = pieceGo.Components.Get<Mineable>();
		// Don't shrink pieces even further.
		mineable.ShrinkRatio = 0f;
	}

	private void SpawnFractureLoot()
	{
		if ( FractureLoot is null )
			return;

		var lootFloat = Random.Shared.Float( 0.7f, 1.2f );
		lootFloat *= Health.MaxHealth / 75f;
		lootFloat *= LootScale;
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
