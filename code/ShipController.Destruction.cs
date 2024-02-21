using Sandbox;
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
		ReleaseDebris();
		DestroyNonDebris();
		DestroyEquipment();
		DeployMeat();
		Rigidbody.Velocity = Vector3.Zero;
		Enabled = false;
	}

	private void ReleaseDebris()
	{
		var parts = PartsContainer.Children
			.Where( c => c.Tags.Has( "break_debris" ) )
			.ToList();
		foreach( var debris in parts )
		{
			var oldTx = debris.Transform.World;
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

	private void DestroyNonDebris()
	{
		foreach( var toDestroy in PartsContainer.Children.ToList() )
		{
			toDestroy.Destroy();
		}
	}

	private void DestroyEquipment()
	{
		var equipment = GameObject.Children
			.Where( c => c.Tags.Has( "equipment" ) )
			.ToList();
		foreach ( var item in equipment )
		{
			item.Destroy();
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
}
