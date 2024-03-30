using Sandbox;
using System.Linq;

public sealed class WorldLoader : Component
{
	[Property] public string World { get; set; }

	protected override void OnStart()
	{
		var worlds = ResourceLibrary.GetAll<WorldMap>();
		var world = worlds.FirstOrDefault( w => w.ResourceName.ToLower() == World.ToLower() );
		if ( world is null )
			return;
		var worldChunker = Scene.GetSystem<WorldChunker>();
		worldChunker.Clear();
		worldChunker.World = world;
	}
}
