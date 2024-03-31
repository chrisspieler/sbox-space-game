using Sandbox;

public static class SceneExtensions
{
	/// <summary>
	/// Returns true if the currently active scene is the main menu scene.
	/// </summary>
	public static bool IsMainMenu( this Scene scene )
	{
		if ( !scene.IsValid() )
			return false;

		return scene.Title == "main_menu";
	}

	/// <summary>
	/// Returns true if the currently active scene is the main scene that is used for gameplay
	/// (i.e. not the main menu or a test scene).
	/// </summary>
	public static bool IsMainGameplayScene( this Scene scene )
	{
		if ( !scene.IsValid() )
			return false;

		return scene.Title == "main";
	}
}
