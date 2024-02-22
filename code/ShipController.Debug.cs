using Sandbox;

public partial class ShipController
{
	[ConVar( "ship_debug" )]
	public static bool Debug { get; set; }
	[ConVar("ship_debug_update_interval")]
	public static float UpdateInterval { get; set; } = 0.15f;


	private TimeSince _lastUpdateTime = 0f;
	private Vector3 _lastPosition;
	private Vector3 _lastVelocity;

	private void UpdateDebugInfo()
	{
		if ( !Debug )
			return;

		if ( _lastUpdateTime >= UpdateInterval )
		{
			_lastPosition = Transform.Position;
			_lastVelocity = Rigidbody?.Velocity ?? Vector3.Zero;
			_lastUpdateTime = 0f;
		}

		Gizmo.Draw.Color = Color.White;

		Gizmo.Draw.ScreenText( $"Position: {_lastPosition}, Velocity: {_lastVelocity}", new Vector2( 25f, 25f ), "Consolas", 12, TextFlag.Left );
		if ( Components.TryGet<FloatingOriginPlayer>( out var floatingOrigin ) )
		{
			var currentChunk = Scene.GetSystem<WorldChunker>().WorldToChunkAbsolute( floatingOrigin.AbsolutePosition );
			Gizmo.Draw.ScreenText( $"Origin Shift: {floatingOrigin.OriginSystem.TotalOriginShift}, Absolute Position: {floatingOrigin.AbsolutePosition}, Current Chunk: {currentChunk}", new Vector2( 25f, 50f ), "Consolas", 12, TextFlag.Left );
		}

		DrawForceArrow( -MainThrusterForce, Color.Blue );
		DrawForceArrow( -RetrorocketForce, Color.Red );
	}

	private void DrawForceArrow( Vector3 force, Color color )
	{
		if ( force.IsNearZeroLength )
			return;

		using ( Gizmo.Scope( "Draw Ship Force Arrow") )
		{
			Gizmo.Draw.Color = color;
			var arrowRay = new Ray( Transform.Position, force.Normal );
			var startPos = arrowRay.Project( 200f );
			var endPos = arrowRay.Project( 200f + force.Length / Time.Delta );
			Gizmo.Draw.Arrow( startPos, endPos, 24, 12 );
		}
	}
}
