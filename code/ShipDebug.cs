using Sandbox;

public sealed class ShipDebug : Component
{
	protected override void OnUpdate()
	{
		Gizmo.Draw.Color = Color.White;
		var velocity = Components.Get<PhysicsComponent>()?.Velocity ?? Vector3.Zero;
		Gizmo.Draw.ScreenText( $"Position: {Transform.Position}, Velocity: {velocity}", new Vector2( 25f, 25f ), "Consolas", 12, TextFlag.Left );
	}
}
