using Sandbox;
using System.Collections.Generic;
using System.Linq;

public class Weapon : Component
{
	public const string AimAssistTargetTag = "target";

	[ConVar( "aim_assist_radius" )]
	public static float AimAssistRadius { get; set; } = 100f;
	[ConVar( "weapon_debug" )]
	public static bool Debug { get; set; } = false;

	[Property] public bool ShouldFire { get; set; }
	[Property] public GameObject WeaponPrefab { get; set; }
	[Property] public List<GameObject> PrefabInstances { get; set; } = new();

	protected Vector3? TargetPosition { get; set; }

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
		var targetGo = FindMouseOverTarget();
		var TargetPosition = Scene.Camera.MouseToWorld();
		if ( Debug )
		{
			using ( Gizmo.Scope( "Swivel Weapons", global::Transform.Zero ) )
			{
				Gizmo.Draw.Color = Color.Red;
				Gizmo.Draw.IgnoreDepth = true;
				Gizmo.Draw.LineSphere( TargetPosition, 10f );
			}
		}
		if ( targetGo is null )
		{
			TargetPosition = AimAssist( TargetPosition );
		}
		foreach( var instance in PrefabInstances )
		{
			var direction = (TargetPosition - instance.Transform.Position).Normal;
			var yaw = Rotation.LookAt( direction ).Yaw();
			instance.Transform.Rotation = Rotation.FromAxis( Transform.Rotation.Up, yaw );
		}
	}

	private GameObject FindMouseOverTarget()
	{
		var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		var tr = Scene.Trace
			.Ray( ray, 5000f )
			.WithTag( AimAssistTargetTag )
			.Run();
		return tr.GameObject;
	}

	private Vector3 AimAssist( Vector3 position )
	{
		var nearby = Scene.FindInPhysics( new Sphere( position, AimAssistRadius ) )
			.Where( go =>
				{
					return go.Tags.Has( AimAssistTargetTag )
						&& go.Transform.Position.Distance( position ) <= AimAssistRadius;
				});
		// If no target is nearby, just use the aim position.
		if ( !nearby.Any() )
			return position;
		// If one or more targets are nearby, use the position of the nearest target.
		var targetPosition = nearby
			.MinBy( go => go.Transform.Position.Distance( position ) )
			.Transform.Position;
		if ( Debug )
		{
			using ( Gizmo.Scope( "Aim Assist", global::Transform.Zero ) )
			{
				Gizmo.Draw.Color = Color.Blue;
				Gizmo.Draw.IgnoreDepth = true;
				Gizmo.Draw.LineSphere( targetPosition, 10f );
			}
		}
		return targetPosition;
	}
}
