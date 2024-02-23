using System.Collections.Generic;
using System.Linq;
using Sandbox;

public static class GameObjectExtensions
{
	public static Vector3 GetAbsolutePosition( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return Vector3.Zero;

		var originSystem = gameObject.Scene.GetSystem<FloatingOriginSystem>();
		return gameObject.Transform.Position + originSystem.TotalOriginShift;
	}

	public static void SetAbsolutePosition( this GameObject gameObject, Vector3 position )
	{
		if ( !gameObject.IsValid() )
			return;

		var originSystem = gameObject.Scene.GetSystem<FloatingOriginSystem>();
		gameObject.Transform.Position = position - originSystem.TotalOriginShift;
	}

	public static void ColorFlash( this GameObject obj, TintEffect effect )
	{
		HashSet<GameObject> reached = new();
		var renderers = obj.Components.GetAll<ModelRenderer>( FindMode.EnabledInSelfAndDescendants ).ToList();
		foreach ( var renderer in renderers )
		{
			if ( reached.Contains( renderer.GameObject ) )
				continue;

			reached.Add( renderer.GameObject );
			var tinter = renderer.GameObject.Components.Get<TintCombiner>( FindMode.EverythingInSelf );
			if ( tinter is null )
			{
				tinter = renderer.GameObject.Components.Create<TintCombiner>();
				tinter.BaseTint = renderer.Tint;
				tinter.Renderer = renderer;
			}
			tinter.Enabled = true;
			tinter.AddTint( effect );
		}
	}
}
