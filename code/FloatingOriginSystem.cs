using System;
using System.Linq;

using Sandbox;

public sealed class FloatingOriginSystem : GameObjectSystem
{
	public static FloatingOriginSystem Instance { get; private set; }

	public event Action<Vector3> OnWorldReset;

	public FloatingOriginSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.PhysicsStep, -1, Tick, "Floating Origin System" );
	}

	private void Tick()
	{
		var player = Scene.GetAllComponents<FloatingOriginPlayer>().FirstOrDefault();

		if ( player is null )
			return;

		if ( player.Transform.Position.Distance( Vector3.Zero) > player.WorldResetDistance )
		{
			ResetWorld( player.GameObject );
		}
	}

	public void ResetWorld( GameObject origin )
	{
		var gameObjects = origin.Scene.Children.Where( go => go != origin );
		var offset = -origin.Transform.Position;
		OnWorldReset?.Invoke( offset );
		origin.Transform.Position = Vector3.Zero;
		foreach ( var go in gameObjects )
		{
			go.Transform.Position += offset;
		}
		Log.Info( $"Floating origin world reset: {offset}" );
	}
}
