using Sandbox;
using System;
using System.Linq;

public sealed partial class ShipController
{
	[ConVar( "ragdoll_wackiness" )]
	public static int MeatRagdollWackiness { get; set; } = 10;

	[Property, Category("Death")] public GameObject Meat { get; set; }
	[Property, Category( "Death" )] public GameObject ExplosionPrefab { get; set; }
	[Property, Category( "Death" )] public SoundEvent ExplosionSound { get; set; }

	[ConCmd("ship_explode")]
	public static void ExplodeCommand()
	{
		var controller = GetCurrent();
		if ( !controller.IsValid() )
		{
			Log.Info( "Unable to find ship controller." );
			return;
		}
		controller.Explode( null );
	}

	public void Explode( DamageInfo damage )
	{
		DestroyEquipment( GameObject );
		ReleaseDebris( PartsContainer );
		DestroyRemainingChildren();
		DeployMeat();
		SpillCargo();
		ReleaseSceneCamera();
		HideHud();
		SpawnExplosion();
		ScreenEffects.AddScreenShake( 1f );
		Rigidbody.Velocity = Vector3.Zero;
		Career.RemoveMoneyCommmand( Career.RespawnFee );
		SaveManager.SaveActiveCareer();
		GameObject.Destroy();
	}

	private void ReleaseDebris( GameObject go )
	{
		var parts = go.Children
			.Where( c => c.Tags.Has( "break_debris" ) && !c.Tags.Has( "break_child" ) )
			.ToList();
		var breakChildren = go.Children
			.Where( c => c.Tags.Has( "break_child" ) )
			.ToList();
		foreach ( var child in breakChildren )
		{
			foreach( var collider in child.Components.GetAll<Collider>( FindMode.EverythingInSelfAndDescendants ) )
			{
				collider.Enabled = true;
			}
		}
		foreach ( var debris in parts )
		{
			ReleaseDebris( debris );

			var oldTx = debris.Transform.World;
			debris.Name = $"(Debris) {debris.Name}";
			debris.Parent = null;
			debris.Transform.World = oldTx;
			// To prevent being ejected by the base shield.
			debris.Tags.Add( "player" );

			// To allow for the ship hitbox to be smaller than the visible model,
			// all colliders for ship parts should remain disabled until they are released as debris.
			if ( debris.Components.TryGet<Collider>( out var collider, FindMode.EverythingInSelf ) )
			{
				collider.Enabled = true;
			}

			if ( debris.Components.TryGet<IDestructionListener>( out var listener ) )
			{
				listener.OnMakeDebris();
			}

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
		var twirler = Meat.Components.Get<RagdollTwirler>();
		twirler.BaseVelocity = Rigidbody.Velocity;
		var deathCamTarget = Scene.GetAllComponents<DeathCamTarget>().FirstOrDefault();
		DeathCam.Begin( deathCamTarget );
		var fog = Scene.GetAllComponents<VolumetricFogVolume>().FirstOrDefault();
		if ( fog is not null )
		{
			// Move the fog from the ship to the death cam.
			fog.GameObject.Parent = null;
			var follower = fog.Components.GetOrCreate<Follower>();
			follower.Target = Scene.Camera.GameObject;
		}
	}

	private void SpillCargo()
	{
		if ( !Cargo.IsValid() )
			return;

		foreach( var item in Cargo.Items )
		{
			var offset = Random.Shared.VectorInSquare( 30f );
			Pickup.Spawn( item, Transform.Position + new Vector3( offset.x, offset.y, 0f ) );
		}
	}

	private void HideHud()
	{
		ScreenManager.SetHudEnabled( false );
		ScreenManager.SetCursorEnabled( false );
		ScreenManager.SetBeaconVisibility( false );
		ScreenManager.SetHoveredSelection( null );
		ScreenManager.ShowDeathScreen();
	}

	private void ReleaseSceneCamera()
	{
		var camera = Scene.Camera;
		// Workaround issue #4926: Orphaned GameObjects have their world position set to their former local position
		var oldPos = camera.Transform.Position;
		camera.GameObject.Parent = null;
		camera.GameObject.Transform.Position = oldPos;
		// Clear all screen shake.
		ScreenEffects.ClearBaseScreenShake();
	}

	private void SpawnExplosion()
	{
		if ( ExplosionPrefab is null )
			return;

		var go = ExplosionPrefab.Clone( Transform.Position );
		go.Name = $"{GameObject.Name} Explosion";
		var mover = go.Components.Create<MoveOnSpawn>();
		mover.AbsolutePosition = GameObject.GetAbsolutePosition();
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
		if ( ExplosionSound is not null )
		{
			Sound.Play( ExplosionSound, Transform.Position );
		}
	}
}
