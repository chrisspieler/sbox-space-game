using Sandbox;

public sealed class MouseSpawner : Component
{
	[Property] public GameObject Prefab { get; set; }

	protected override void OnUpdate()
	{
		if ( !Input.Pressed( "attack1" ) )
			return;

		var mouseRay = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		var plane = new Plane( Vector3.Zero, Vector3.Up );
		var worldPos = plane.Trace( mouseRay );
		if ( !worldPos.HasValue )
			return;
		var go = Prefab.Clone( worldPos.Value );
		go.BreakFromPrefab();
	}
}
