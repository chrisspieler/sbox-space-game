using Sandbox;

public class DistanceRecord : GameObjectSystem
{
	public DistanceRecord( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, OnUpdate, "Distance Record Checker" );
	}

	public bool NewRecordPending { get; private set; }

	private TimeSince _lastRecordNotification;

	private void OnUpdate()
	{
		if ( Career.Active is null || !Scene.IsMainGameplayScene() )
			return;

		var ship = ShipController.GetCurrent();
		if ( !ship.IsValid() )
		{
			// If we had a new record pending, but the ship was destroyed...
			if ( NewRecordPending )
			{
				// ...show it in the middle of the screen.
				var position = Scene.Camera.ScreenNormalToWorld( 0.5f );
				ShowNewRecordMessage( position );
			}
			return;
		}

		var currentDistance = ship.GameObject
			.GetAbsolutePosition()
			.Distance( Vector3.Zero );
		currentDistance = MathX.InchToMeter( currentDistance );
		if ( currentDistance > Career.Active.FarthestDistance )
		{
			NewRecordPending = true;
			Career.Active.FarthestDistance = currentDistance;
		}
		if ( ShouldShowNewRecordMessage() )
		{
			var position = ShipController.GetCurrent().Transform.Position;
			ShowNewRecordMessage( position );
		}
	}

	private bool ShouldShowNewRecordMessage()
	{
		if ( !NewRecordPending || _lastRecordNotification < 5f )
			return false;

		var ship = ShipController.GetCurrent();
		var dirToOrigin = ship.GameObject.GetAbsolutePosition().Direction( Vector3.Zero );
		return ship.Rigidbody.Velocity.Dot( dirToOrigin ) > 0;
	}

	private void ShowNewRecordMessage( Vector3 position )
	{
		_lastRecordNotification = 0f;
		var message = $"New Distance Record: { Metric.FormatDistance( Career.Active.FarthestDistance ) }";
		ScreenManager.ShowTextPanel( message, position, false, 5f );
		SaveManager.SaveActiveCareer();
		NewRecordPending = false;
	}
}
