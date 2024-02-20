using System;
using System.Linq;

using Sandbox;

public sealed class FloatingOriginSystem : GameObjectSystem
{
	[ConVar( "origin_shift_debug" )]
	public static bool Debug { get; set; } 

	[ConVar( "origin_shift_distance" )]
	public static float OriginShiftDistance { get; set; } = 16_000f;

	public FloatingOriginPlayer Origin { get; private set; }
	public Vector3 TotalOriginShift { get; private set; }
	public event Action<Vector3> OnWorldReset;

	public FloatingOriginSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.PhysicsStep, 0, Tick, "Floating Origin System" );
	}

	public Vector3 RelativeToAbsolute( Vector3 relativePos )
	{
		return Origin != null 
			? relativePos + TotalOriginShift
			: relativePos;
	}
	public Vector3 AbsoluteToRelative( Vector3 absolutePos )
	{
		return Origin != null 
			? absolutePos - TotalOriginShift
			: absolutePos;
	}

	private void Tick()
	{
		Origin = Scene.GetAllComponents<FloatingOriginPlayer>().FirstOrDefault();

		if ( Origin is null )
			return;

		if ( Origin.Transform.Position.Distance( Vector3.Zero) > OriginShiftDistance )
		{
			ResetWorld( Origin.GameObject );
		}
	}

	public void ResetWorld( GameObject origin )
	{
		var gameObjects = origin.Scene.Children.Where( go => go != origin );
		var offset = -origin.Transform.Position;
		TotalOriginShift -= offset;
		OnWorldReset?.Invoke( offset );
		origin.Transform.Position = Vector3.Zero;
		if ( Debug )
		{
			Gizmo.Draw.FollowText( $"Shifted Origin: {offset}", origin );
		}
		foreach ( var go in gameObjects )
		{
			go.Transform.Position += offset;
		}
	}
}
