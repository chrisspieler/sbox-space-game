using Sandbox;
using System;

public static class ScreenEffects
{
	public static void SetBloom( float amount )
	{
		var postProcessing = PostProcessingController.GetCurrent();
		if ( !postProcessing.IsValid() )
			return;

		postProcessing.TargetBloom = amount;
	}

	public static void SetSharpness( float amount )
	{
		var postProcessing = PostProcessingController.GetCurrent();
		if ( !postProcessing.IsValid() )
			return;

		postProcessing.TargetSharpness = amount;
	}

	public static void AddScreenShake( float amount, float max = 1f )
	{
		var screenShake = ScreenShake.Instance;
		if ( !screenShake.IsValid() )
			return;

		if ( screenShake.Trauma >= max )
			return;

		screenShake.Trauma = Math.Min( screenShake.Trauma + amount, max );
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
