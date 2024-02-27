using Sandbox;

public static class CameraComponentExtensions
{
	public static Vector3 MouseToWorld( this CameraComponent camera )
	{
		if ( GameManager.ActiveScene?.Camera is null )
			return Vector3.Zero;

		var mouseNormalPos = Mouse.Position / Screen.Size;
		var ray = GameManager.ActiveScene.Camera.ScreenNormalToRay( mouseNormalPos );
		var worldPlane = new Plane( Vector3.Zero, Vector3.Up );
		return worldPlane.Trace( ray ) ?? Vector3.Zero;
	}
}
