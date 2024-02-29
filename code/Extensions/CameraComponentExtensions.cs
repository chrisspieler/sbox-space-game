using Sandbox;

public static class CameraComponentExtensions
{
	public static Vector3 MouseToWorld( this CameraComponent camera )
	{
		var mouseNormalPos = Mouse.Position / Screen.Size;
		return camera.ScreenNormalToWorld( mouseNormalPos );
	}

	public static Vector3 ScreenNormalToWorld( this CameraComponent camera, Vector2 normal )
	{
		var centerRay = camera.ScreenNormalToRay( normal );
		var worldPlane = new Plane( Vector3.Zero, Vector3.Up );
		return worldPlane.Trace( centerRay ) ?? Vector3.Zero;
	}
}
