using Sandbox;

public sealed class ShipDebug : Component
{
	[Property] public float UpdateInterval { get; set; } = 0.15f;
	

	private TimeSince _lastUpdateTime = 0f;
	private Vector3 _lastPosition;
	private Vector3 _lastVelocity;
	private float _forceArrowDistance { get; set; } = 100f;


	protected override void OnStart()
	{
		base.OnStart();

		var bounds = GameObject.GetBounds();
		var averageSize = ( bounds.Maxs - bounds.Mins ).Length / 2f;
		_forceArrowDistance = averageSize;
	}

	protected override void OnUpdate()
	{
		if ( _lastUpdateTime >= UpdateInterval )
		{
			_lastPosition = Transform.Position;
			_lastVelocity = Components.Get<Rigidbody>()?.Velocity ?? Vector3.Zero;
			_lastUpdateTime = 0f;
		}

		Gizmo.Draw.Color = Color.White;
		
		Gizmo.Draw.ScreenText( $"Position: {_lastPosition}, Velocity: {_lastVelocity}", new Vector2( 25f, 25f ), "Consolas", 12, TextFlag.Left );
		if ( Components.TryGet<FloatingOriginPlayer>( out var floatingOrigin ) )
		{
			Gizmo.Draw.ScreenText( $"Origin Shift: {floatingOrigin.TotalOriginShift}", new Vector2( 25f, 50f ), "Consolas", 12, TextFlag.Left );
		}
		
		var shipController = Components.Get<ShipController>();
		if ( shipController is null )
			return;

		DrawForceArrow( -shipController.MainThrusterForce.Normal * 2f, Color.Blue );
		DrawForceArrow( -shipController.RetrorocketForce.Normal * 3f, Color.Red );
	}

	private void DrawForceArrow( Vector3 force, Color color )
	{
		if ( force.IsNearZeroLength )
			return;

		var previousColor = Gizmo.Draw.Color;
		Gizmo.Draw.Color = color;
		var forceArrowDistance = _forceArrowDistance + 30f;
		var arrowStart = Transform.Position + force.Normal * forceArrowDistance;
		var transform = new Transform()
			.WithPosition( arrowStart )
			.WithRotation( Rotation.LookAt( force ) )
			.WithScale( force.Length );
		Gizmo.Draw.Model( "models/editor/arrow", transform );
		Gizmo.Draw.Color = previousColor;
	}
}
