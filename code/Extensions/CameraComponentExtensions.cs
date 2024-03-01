using Sandbox;

public static class CameraComponentExtensions
{
	public static Vector3 MouseToWorld( this CameraComponent camera )
	{
		return camera.ScreenPixelToWorld( Mouse.Position );
	}

	public static Vector3 ScreenPixelToWorld( this CameraComponent camera, Vector2 position )
	{
		return camera.ScreenRayToWorld( camera.ScreenPixelToRay( position ) );
	}

	public static Vector3 ScreenNormalToWorld( this CameraComponent camera, Vector2 normal )
	{
		return camera.ScreenRayToWorld( camera.ScreenNormalToRay( normal ) );
	}

	public static Vector3 ScreenRayToWorld( this CameraComponent camera, Ray ray )
	{
		var worldPlane = new Plane( Vector3.Zero, Vector3.Up );
		return worldPlane.Trace( ray ) ?? Vector3.Zero;
	}
}
