using Sandbox;

public class EditorPause : GameObjectSystem
{
	public EditorPause( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, OnTick, "Check Pause" );
	}

	private void OnTick()
	{
		if ( Game.IsEditor && Input.Pressed( "pause" ) )
		{
			Game.IsPaused = !Game.IsPaused;
		}
	}
}
