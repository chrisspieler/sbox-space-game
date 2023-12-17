namespace Sandbox;

public static class GameObjectExtensions
{
	public static Vector3 GetAbsolutePosition( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return Vector3.Zero;

		var originSystem = gameObject.Scene.GetSystem<FloatingOriginSystem>();
		return gameObject.Transform.Position + originSystem.TotalOriginShift;
	}

	public static void SetAbsolutePosition( this GameObject gameObject, Vector3 position )
	{
		if ( !gameObject.IsValid() )
			return;

		var originSystem = gameObject.Scene.GetSystem<FloatingOriginSystem>();
		gameObject.Transform.Position = position - originSystem.TotalOriginShift;
	}
}
