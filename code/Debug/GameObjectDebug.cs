using Sandbox;

public class GameObjectDebug : GameObjectSystem
{
	[ConVar( "gameobject_show_tags" )]
	public static bool ShowTags { get; set; }

	public GameObjectDebug( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, Tick, "GameObject Debug" );
	}

	private void Tick()
	{
		if ( !ShowTags )
			return;

		using ( Gizmo.Scope( "GameObject Tags" ) )
		{
			Gizmo.Draw.Color = Color.Yellow;
			foreach ( var go in Scene.GetAllObjects( true ) )
			{
				Gizmo.Transform = go.Transform.World;
				Gizmo.Draw.Text( $"{go.Name}", Transform.Zero, "Consolas" );
			}
		}
	}

}
