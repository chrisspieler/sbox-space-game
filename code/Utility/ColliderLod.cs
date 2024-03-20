using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class ColliderLod : Component
{
	[ConVar( "collider_lod" )]
	public static bool EnableColliderLodding { get; set; } = true;
	[ConVar( "collider_lod_lowdetail_distance" )]
	public static float DefaultLowDetailDistance { get; set; } = 2500f;
	[ConVar( "collider_lod_disable_distance" )]
	public static float DefaultCollisionDisableDistance { get; set; } = 3500f;

	[Property] public List<Collider> HighDetail { get; set; }
	[Property] public List<Collider> LowDetail { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float? LowDetailDistance { get; set; }
	[Property] public float? CollisionDisableDistance { get; set; }

	protected override void OnUpdate()
	{
		Target = FloatingOriginPlayer.Instance?.GameObject ?? Scene.Camera?.GameObject;

		if ( !HighDetail.Any() || !LowDetail.Any() || !Target.IsValid() )
			return;

		if ( !EnableColliderLodding )
		{
			UseHighLod();
			return;
		}

		var collisionDisableDistance = CollisionDisableDistance ?? DefaultCollisionDisableDistance;
		var lowDetailDistance = LowDetailDistance ?? DefaultLowDetailDistance;
		var distance = Transform.Position.Distance( Target.Transform.Position );
		if ( distance < lowDetailDistance )
		{
			UseHighLod();
		}
		else if ( distance < collisionDisableDistance )
		{
			UseLowLod();
		}
		else
		{
			DisableCollision();
		}
	}

	private void UseHighLod()
	{
		Rigidbody.SetEnabled( true );
		foreach( var lowDetail in LowDetail )
		{
			lowDetail.Enabled = false;
		}
		foreach( var highDetail in HighDetail )
		{
			highDetail.Enabled = true;
		}
	}

	private void UseLowLod()
	{
		Rigidbody.SetEnabled( true );
		foreach ( var highDetail in HighDetail )
		{
			highDetail.Enabled = false;
		}
		foreach( var lowDetail in LowDetail )
		{
			lowDetail.Enabled = true;
		}
	}

	private void DisableCollision()
	{
		Rigidbody.SetEnabled( false );
		foreach ( var highDetail in HighDetail )
		{
			highDetail.Enabled = false;
		}
		foreach ( var lowDetail in LowDetail )
		{
			lowDetail.Enabled = false;
		}
	}
}
