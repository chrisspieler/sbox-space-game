using Sandbox;

public sealed class BallMode : Component
{
	[ConCmd("ball_mode")]
	public static void EnterBallMode()
	{
		if ( Game.ActiveScene is null )
			return;

		Game.ActiveScene.Load( ResourceLibrary.Get<SceneFile>( "scenes/ball_mode.scene" ) );
	}
}
