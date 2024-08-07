﻿using Sandbox;
using System;
using System.Linq;

public sealed partial class ShipController
{
	[ConVar( "ragdoll_wackiness" )]
	public static int MeatRagdollWackiness { get; set; } = 10;

	[Property, Category( "Death" )] public DebrisConverter Debris { get; set; }
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
		controller.Explode( null );
	}

	public void Explode( DamageInfo damage )
	{
		DestroyEquipment( GameObject );
		Debris.ReleaseDebris();
		DestroyRemainingChildren();
		DeployMeat();
		SpillCargo();
		ReleaseSceneCamera();
		HideHud();
		SpawnExplosion();
		CreateBeacon();
		ScreenEffects.AddScreenShake( 1f );
		Rigidbody.Velocity = Vector3.Zero;
		Career.Active.RemoveMoney( Career.RespawnFee );
		UpdateExplosionStat();
		SaveManager.SaveActiveCareer();
		GameObject.Destroy();
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
		DeathCam.Begin( Meat );
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
		go.Transform.ClearInterpolation();
		go.Name = $"{GameObject.Name} Explosion";
		go.Tags.Add( "no_chunk" );
		var particleEffects = go.Components.GetAll<ParticleEffect>( FindMode.EnabledInSelfAndDescendants );
		foreach( var effect in particleEffects )
		{
			// For effects that use force, inherit velocity from the ship.
			effect.ForceDirection = (effect.ForceDirection + Rigidbody.Velocity.Normal).Normal;
			effect.ForceScale = effect.ForceScale.ConstantValue + Rigidbody.Velocity.Length;
		}
	}

	private void UpdateExplosionStat()
	{
		if ( !CheatManager.HasCheated && Scene.IsMainGameplayScene() )
		{
			Sandbox.Services.Stats.Increment( "ship-explosions", 1 );
		}
		if ( Career.Active is not null )
		{
			Career.Active.ShipExplosions++;
		}
	}

	private void CreateBeacon()
	{
		Beacon.Create( Transform.Position, "SHIPWRECK", "player_shipwreck", 500f );
	}
}
