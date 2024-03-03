using Sandbox;

public static class ScreenEffects
{
	public static void AddScreenShake( float amount )
	{
		var camera = ShipCamera.GetCurrent();
		if ( !camera.IsValid() )
			return;

		camera.Trauma += amount;
	}

	public static void SetBaseScreenShake( IValid shaker, float amount, bool enable )
	{
		var camera = ShipCamera.GetCurrent();
		if ( !camera.IsValid() )
			return;

		camera.SetBaseScreenShake( shaker, amount, enable );
	}

	public static void ClearBaseScreenShake( IValid shaker )
	{
		var camera = ShipCamera.GetCurrent();
		if ( !camera.IsValid() )
			return;

		camera.SetBaseScreenShake( shaker, 0f, false );
	}
}
