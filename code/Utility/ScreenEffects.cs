using Sandbox;

public static class ScreenEffects
{
	public static void AddScreenShake( float amount )
	{
		var screenShake = ScreenShake.Instance;
		if ( !screenShake.IsValid() )
			return;

		screenShake.Trauma += amount;
	}

	public static void SetBaseScreenShake( IValid shaker, float amount, bool enable )
	{
		var screenShake = ScreenShake.Instance;
		if ( !screenShake.IsValid() )
			return;

		screenShake.SetBaseScreenShake( shaker, amount, enable );
	}

	public static void ClearBaseScreenShake()
	{
		var screenShake = ScreenShake.Instance;
		if ( !screenShake.IsValid() )
			return;

		screenShake.ClearAllScreenShakeToggles();
	}

	public static void ClearBaseScreenShake( IValid shaker )
	{
		var screenShake = ScreenShake.Instance;
		if ( !screenShake.IsValid() )
			return;

		screenShake.SetBaseScreenShake( shaker, 0f, false );
	}
}
