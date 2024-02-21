using Sandbox;
using System;
using System.Linq;

public sealed partial class ShipController
{
	[Property, Category("Death")] public GameObject Meat { get; set; }
	[Property, Category("Death")] public VolumetricFogVolume Fog { get; set; }

	[ConCmd("ship_explode")]
	public static void ExplodeCommand()
	{
		var controller = GameManager.ActiveScene
			.GetAllComponents<ShipController>()
			.FirstOrDefault();
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
		ReleaseDebris();
		DestroyRemainingChildren();
		DeployMeat();
		SpillCargo();
		Scene.Camera.GameObject.Parent = null;
		HideHud();
		Rigidbody.Velocity = Vector3.Zero;
		GameObject.Destroy();
	}

	private void ReleaseDebris()
	{
		var parts = PartsContainer.Children
			.Where( c => c.Tags.Has( "break_debris" ) )
			.ToList();
		foreach( var debris in parts )
		{
			var oldTx = debris.Transform.World;
			debris.Name = $"(Debris) {debris.Name}";
			debris.Parent = null;
			debris.Transform.World = oldTx;
			debris.Tags.Add( "solid" );
			var rb = debris.Components.Get<Rigidbody>( true );
			rb.Enabled = true;
			rb.Velocity = Rigidbody.Velocity;
			rb.Velocity += Vector3.Random.WithZ( 0f ) * 200f;
			rb.AngularVelocity += Vector3.Random * 3f;
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
			if ( item.Tags.Has( "equipment" ) )
			{
				item.Destroy();
			}
			else
			{
				DestroyEquipment( item );
			}
		}
	}

	private void DeployMeat()
	{
		var oldTx = Meat.Transform.World;
		Meat.Parent = null;
		Meat.Transform.World = oldTx;
		Meat.Enabled = true;
		var phys = Meat.Components.Get<ModelPhysics>();
		phys.PhysicsGroup.AddVelocity( Vector3.Random * 100f );
		phys.PhysicsGroup.AddAngularVelocity( Vector3.Random * 100f );
		var renderer = Meat.Components.Get<SkinnedModelRenderer>();
		renderer.SceneModel.UseAnimGraph = false;
		var deathCam = Meat.Components.GetInDescendantsOrSelf<DeathCamConfig>( true );
		deathCam.Enabled = true;
		// Move the fog from the ship to the death cam.
		Fog.GameObject.Parent = null;
		var follower = Fog.Components.Create<Follower>();
		follower.Target = deathCam.GameObject;
	}

	private void SpillCargo()
	{
		for (int i = 0; i < 20; i ++ )
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/obstacles/lightball.prefab" );
			var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
			var go = prefabScene.Clone();
			go.Transform.Position = Transform.Position + Random.Shared.VectorInSphere( 20f );
			var rb = go.Components.Get<Rigidbody>();
			rb.Velocity = Rigidbody.Velocity;
			rb.Velocity += Vector3.Random * Random.Shared.Float( 20f, 150f );
		}
	}

	private void HideHud()
	{
		Component hud = Scene.GetAllComponents<HudPanel>().FirstOrDefault();
		hud.Enabled = false;
		Component cursor = Scene.GetAllComponents<CursorPanel>().FirstOrDefault();
		cursor.Enabled = false;
	}
}
