using Sandbox;
using System;
using System.Linq;

public sealed partial class ShipController
{
	[ConVar( "ragdoll_wackiness" )]
	public static int MeatRagdollWackiness { get; set; } = 10;

	[Property, Category("Death")] public GameObject Meat { get; set; }
	[Property, Category( "Death" )] public GameObject ExplosionPrefab { get; set; }

	[ConCmd("ship_explode")]
	public static void ExplodeCommand()
	{
		var controller = GetCurrent();
		if ( !controller.IsValid() )
		{
			Log.Info( "Unable to find ship controller." );
			return;
		}
		controller.Explode();
	}

	public void Explode()
	{
		DestroyEquipment( GameObject );
		ReleaseDebris( PartsContainer );
		DestroyRemainingChildren();
		DeployMeat();
		SpillCargo();
		ReleaseSceneCamera();
		HideHud();
		SpawnExplosion();
		Rigidbody.Velocity = Vector3.Zero;

		GameObject.Destroy();
	}

	private void ReleaseDebris( GameObject go )
	{
		var parts = go.Children
			.Where( c => c.Tags.Has( "break_debris" ) && !c.Tags.Has( "break_child" ) )
			.ToList();
		foreach( var debris in parts )
		{
			ReleaseDebris( debris );

			var oldTx = debris.Transform.World;
			debris.Name = $"(Debris) {debris.Name}";
			debris.Parent = null;
			debris.Transform.World = oldTx;
			// To prevent being ejected by the base shield.
			debris.Tags.Add( "player" );

			if ( debris.Components.TryGet<Rigidbody>(out var rb, FindMode.EverythingInSelf ) )
			{
				rb.Enabled = true;
				rb.Velocity = Rigidbody.Velocity;
				rb.Velocity += Vector3.Random.WithZ( 0f ) * 200f;
				// Make sure the mass of each piece of ship debris is low enough that the ship can push it.
				rb.PhysicsBody.Mass = Math.Min( rb.PhysicsBody.Mass, 100f );
				rb.AngularVelocity += Vector3.Random * 3f;
			}
		}
	}

	private void DestroyRemainingChildren()
	{
		foreach( var toDestroy in PartsContainer.Children.ToList() )
		{
			toDestroy.Destroy();
		}
	}

	private void DestroyEquipment( GameObject go )
	{
		foreach ( var item in go.Children.ToList() )
		{
			DestroyEquipment( item );

			if ( item.Tags.Has( "equipment" ) )
			{
				item.Destroy();
			}
		}
	}

	private void DeployMeat()
	{
		var oldTx = Meat.Transform.World;
		Meat.Parent = null;
		Meat.Transform.World = oldTx;
		Meat.Enabled = true;
		// To avoid being ejected by the base shield.
		Meat.Tags.Add( "player" );
		var renderer = Meat.Components.Get<SkinnedModelRenderer>();
		// Stop animations so the meat does not blink.
		renderer.SceneModel.UseAnimGraph = false;
		ApplyRagdollTwirl( Meat, MeatRagdollWackiness );
		var deathCam = Meat.Components.GetInDescendantsOrSelf<DeathCamConfig>( true );
		deathCam.Enabled = true;
		var fog = Scene.GetAllComponents<VolumetricFogVolume>().First();
		// Move the fog from the ship to the death cam.
		fog.GameObject.Parent = null;
		var follower = fog.Components.GetOrCreate<Follower>();
		follower.Target = deathCam.GameObject;
	}

	private void ApplyRagdollTwirl( GameObject ragdoll, int wackiness = 10 )
	{
		var phys = ragdoll.Components.Get<ModelPhysics>();
		if ( !phys.IsValid() )
			return;

		for( int i = 0; i < wackiness; i++ )
		{
			var randomVelocity = (Vector3.Random * 100f).WithZ( 0f );
			randomVelocity *= Random.Shared.Float( 0.5f, 2f );
			var randomBodyIndex = Random.Shared.Int( 0, phys.PhysicsGroup.BodyCount - 1 );
			var randomBody = phys.PhysicsGroup.GetBody( randomBodyIndex );
			randomBody.Velocity += randomVelocity + Rigidbody.Velocity / 2;
			randomBody.AngularVelocity += Vector3.Random * 100f * Random.Shared.Float( 0.5f, 2f );
		}
	}

	private void SpillCargo()
	{
		// TODO: Empty the cargo hold out in to the world.
	}

	private void HideHud()
	{
		ScreenManager.SetHudEnabled( false );
		ScreenManager.SetCursorEnabled( false );
		ScreenManager.ShowDeathScreen();
	}

	private void ReleaseSceneCamera()
	{
		var camera = Scene.Camera;
		// Workaround issue #4926: Orphaned GameObjects have their world position set to their former local position
		var oldPos = camera.Transform.Position;
		camera.GameObject.Parent = null;
		camera.GameObject.Transform.Position = oldPos;
	}

	private void SpawnExplosion()
	{
		if ( ExplosionPrefab is null )
			return;

		var go = ExplosionPrefab.Clone( Transform.Position );
		go.Name = $"{GameObject.Name} Explosion";
		var particleEffects = go.Components.GetAll<ParticleEffect>( FindMode.EnabledInSelfAndDescendants );
		foreach( var effect in particleEffects )
		{
			// For effects that use force, inherit velocity from the ship.
			effect.ForceDirection = (effect.ForceDirection + Rigidbody.Velocity.Normal).Normal;
			effect.ForceScale = effect.ForceScale.ConstantValue + Rigidbody.Velocity.Length;
			effect.CollisionIgnore ??= new TagSet();
			// Don't collide with released debris.
			effect.CollisionIgnore.Add( "player" );
			// Don't collide with the base shield.
			effect.CollisionIgnore.Add( "player_shield" );
		}
	}
}
