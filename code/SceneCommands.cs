using System.Linq;
using Sandbox;

public static class SceneCommands
{
	[ConCmd( "scene" )]
	public static void LoadScene( string sceneName)
	{
		if ( Game.ActiveScene is null )
			return;

		var allScenes = ResourceLibrary.GetAll<SceneFile>();
		var scene = allScenes.FirstOrDefault( s => s.ResourceName.ToLower() == sceneName.ToLower() );
		if ( scene is null )
		{
			Log.Error( $"No such scene exists: {sceneName}" );
		}
		var worldChunker = Game.ActiveScene.GetSystem<WorldChunker>();
		worldChunker.Clear();
		worldChunker.World = null;
		Game.ActiveScene.Load( scene );
	}

	[ConCmd( "scene_list" )]
	public static void SceneList()
	{
		var allScenes = ResourceLibrary.GetAll<SceneFile>();
		foreach( var scene in allScenes.OrderBy( s => s.ResourceName ) )
		{
			Log.Info( $"{scene.ResourceName}" );
		}
	}
}
