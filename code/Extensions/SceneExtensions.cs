using Sandbox;

public static class SceneExtensions
{
	public static bool IsMainMenu( this Scene scene )
	{
		if ( !scene.IsValid() )
			return false;

		return scene.Title == "main_menu";
	}
}
