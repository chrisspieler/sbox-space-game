using System.Collections.Generic;
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

	public FloatingOriginSystem( Scene scene ) : base( scene )
	{
		// If an origin shift occurs outside of PhysicsStep, rigidbodies may jump around unexpectedly.
		Listen( Stage.PhysicsStep, 0, Tick, "Floating Origin System" );
	}

	public Vector3 RelativeToAbsolute( Vector3 relativePos )
	{
		return relativePos + TotalOriginShift;
	}

	public Vector3 AbsoluteToRelative( Vector3 absolutePos )
	{
		return absolutePos - TotalOriginShift;
	}

	private void Tick()
	{
		Origin = Scene.GetAllComponents<FloatingOriginPlayer>().FirstOrDefault();

		if ( Origin is null )
			return;

		if ( Origin.Transform.Position.Distance( Vector3.Zero ) > OriginShiftDistance )
		{
			DoOriginShift();
		}
	}

	public void DoOriginShift()
	{
		var listeners = Scene.GetAllComponents<IOriginShiftListener>().ToList();
		var offset = -Origin.Transform.Position;
		BeforeOriginShift( offset, listeners );
		ShiftWorld( Origin.GameObject, offset );
		AfterOriginShift( offset, listeners );
		return;
	}

	public void ShiftWorld( GameObject origin, Vector3 offset )
	{
		TotalOriginShift -= offset;
		var gameObjects = origin.Scene.Children.Where( go => !go.Tags.Has("no_shift") );
		foreach ( var go in gameObjects )
		{
			if ( Debug )
			{
				Gizmo.Draw.FollowText( $"({go.Name}) Shifted Origin: {offset}", go );
			}
			go.Transform.Position += offset;
			go.Transform.ClearLerp();
		}
	}

	private void BeforeOriginShift( Vector3 offset, List<IOriginShiftListener> listeners )
	{
		foreach ( var shifted in listeners )
		{
			shifted.OnBeforeOriginShift( offset );
		}
	}

	private void AfterOriginShift( Vector3 offset, List<IOriginShiftListener> listeners )
	{
		if ( Debug )
		{
			Gizmo.Draw.FollowText( $"Shifted Origin: {offset}", Origin.GameObject );
		}
		foreach ( var shifted in listeners )
		{
			// It's possible that an origin shift listener was invalidated during BeforeOriginShift.
			if ( !(shifted as Component).IsValid() )
				continue;

			shifted.OnAfterOriginShift( offset );
		}
	}
}
