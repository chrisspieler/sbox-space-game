using Sandbox;
using System;

public sealed class AsteroidFracture : Component
{
	[ConVar( "asteroid_fracture_debug" )]
	public static bool Debug { get; set; } = false;

	[Property] public Health Health { get; set; }
	[Property] public Asteroid Asteroid { get; set; }

	protected override void OnStart()
	{
		Health ??= Components.Get<Health>();
		Asteroid ??= Components.Get<Asteroid>();
		Health.OnKilled += _ => SpawnFracturePieces();
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
		for ( int i = 0; i < pieceCount; i++ )
		{
			var pieceScale = scale / pieceCount;
			SpawnFragment( pieceScale );
		}
	}

	private void SpawnFragment( Vector3 scale )
	{
		var bounds = GameObject.GetBounds();
		bounds.Maxs -= bounds.Size / 2f;
		bounds.Mins += bounds.Size / 2f;
		var pieceGo = Asteroid.Create( Asteroid.Data, scale.x );
		pieceGo.Transform.Position = bounds.RandomPointInside.WithZ( 0f );
		// Stop the floater from interfering.
		if ( pieceGo.Components.TryGet<SpaceFloater>( out var floater ) )
		{
			floater.Enabled = false;
			floater.Destroy();
		}
		var randomPos = Random.Shared.VectorInSquare( Transform.Scale.Length / 2f );
		pieceGo.Transform.Position += new Vector3( randomPos.x, randomPos.y );
		pieceGo.Transform.Rotation = Rotation.Random;
		if ( Debug )
		{
			Log.Info( $"Spawning fragment with scale {scale} at {pieceGo.Transform.Position}" );
		}
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
}
