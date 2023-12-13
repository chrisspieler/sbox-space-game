using Sandbox;

public sealed class ShipDebug : Component
{
	[Property] public float UpdateInterval { get; set; } = 0.15f;
	private TimeSince _lastUpdateTime = 0f;
	private Vector3 _lastPosition;
	private Vector3 _lastVelocity;

	protected override void OnUpdate()
	{
		if ( _lastUpdateTime >= UpdateInterval )
		{
			_lastPosition = Transform.Position;
			_lastVelocity = Components.Get<PhysicsComponent>()?.Velocity ?? Vector3.Zero;
			_lastUpdateTime = 0f;
		}

		Gizmo.Draw.Color = Color.White;
		
		Gizmo.Draw.ScreenText( $"Position: {_lastPosition}, Velocity: {_lastVelocity}", new Vector2( 25f, 25f ), "Consolas", 12, TextFlag.Left );
	}
}
