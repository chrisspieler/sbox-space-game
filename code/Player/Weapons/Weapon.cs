using Sandbox;
using System.Collections.Generic;

public class Weapon : Component
{
	[Property] public bool ShouldFire { get; set; }
	[Property] public GameObject WeaponPrefab { get; set; }
	[Property] public List<GameObject> PrefabInstances { get; set; } = new();

	public ShipController Ship { get; set; }

	protected override void OnEnabled()
	{
		Ship = Components.GetInAncestors<ShipController>();
		Ship.ActiveWeapon.SetEnabled( false );
		Ship.ActiveWeapon = this;
		CreateWeaponPrefab();
	}

	protected override void OnDestroy()
	{
		DestroyWeaponPrefab();
	}

	protected virtual void CreateWeaponPrefab()
	{
		CreateAtTarget( Ship.WeaponPointLeft );
		CreateAtTarget( Ship.WeaponPointRight );

		void CreateAtTarget( GameObject target )
		{
			var tx = target.Transform.World.WithScale( 1f );
			var go = WeaponPrefab.Clone( tx );
			go.Parent = target;
			go.Transform.World = tx;
			go.BreakFromPrefab();
			OnPrefabInstanceAdded( go );
			PrefabInstances.Add( go );
		}
	}

	protected virtual void DestroyWeaponPrefab()
	{
		foreach( var instance in PrefabInstances )
		{
			instance.Destroy();
		}
	}

	protected virtual void OnPrefabInstanceAdded( GameObject instance ) { }

	protected virtual void SwivelWeapons()
	{
		foreach( var instance in PrefabInstances )
		{
			var mousePos = Scene.Camera.MouseToWorld();
			var direction = (mousePos - instance.Transform.Position).Normal;
			var yaw = Rotation.LookAt( direction ).Yaw();
			instance.Transform.Rotation = ((Angles)instance.Transform.Rotation).WithYaw( yaw );
		}
	}
}
