using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class ColliderLod : Component
{
	[ConVar( "collider_lod" )]
	public static bool EnableColliderLodding { get; set; } = true;
	[ConVar( "collider_lod_distance" )]
	public static float DefaultLodDistance { get; set; } = 2500f;

	[Property] public List<Collider> HighDetail { get; set; }
	[Property] public List<Collider> LowDetail { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public float Distance { get; set; }

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

		var distance = Distance == 0f ? DefaultLodDistance : Distance;
		if ( Transform.Position.Distance( Target.Transform.Position ) < distance )
		{
			UseHighLod();
		}
		else
		{
			UseLowLod();
		}
	}

	private void UseHighLod()
	{
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
		foreach( var highDetail in HighDetail )
		{
			highDetail.Enabled = false;
		}
		foreach( var lowDetail in LowDetail )
		{
			lowDetail.Enabled = true;
		}
	}
}
