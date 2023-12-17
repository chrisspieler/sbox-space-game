using System;
using System.Linq;

using Sandbox;

public sealed class FloatingOriginSystem : GameObjectSystem
{
	public FloatingOriginPlayer Origin { get; private set; }
	public Vector3 TotalOriginShift { get; private set; }
	public event Action<Vector3> OnWorldReset;

	public FloatingOriginSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.PhysicsStep, -1, Tick, "Floating Origin System" );
	}

	private void Tick()
	{
		Origin = Scene.GetAllComponents<FloatingOriginPlayer>().FirstOrDefault();

		if ( Origin is null )
			return;

		if ( Origin.Transform.Position.Distance( Vector3.Zero) > Origin.WorldResetDistance )
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
		foreach ( var go in gameObjects )
		{
			go.Transform.Position += offset;
		}
		Log.Info( $"Floating origin world reset: {offset}" );
	}
}
