using System.Collections.Generic;
using System.Linq;
using Sandbox;

public static class GameObjectExtensions
{
	public static void SetEnabled( this GameObject gameObject, bool enabled )
	{
		if ( gameObject is null )
			return;

		gameObject.Enabled = enabled;
	}

	public static bool ToggleEnabled( this GameObject gameObject )
	{
		if ( gameObject is null )
			return false;

		gameObject.Enabled = !gameObject.Enabled;
		return gameObject.Enabled;
	}

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
		gameObject.Transform.Position = originSystem.AbsoluteToRelative( position );
	}

	public static float GetDistanceFromScreenCenter( this GameObject gameObject )
	{
		return Game.ActiveScene
				.Camera
				.ScreenNormalToWorld( 0.5f )
				.Distance( gameObject.Transform.Position );
	}

	/// <summary>
	/// Returns the position of the specified <see cref="GameObject"/> relative to the 
	/// chunk that contains it.
	/// </summary>
	public static Vector2 GetChunkLocalPosition( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return Vector2.Zero;

		var chunkPos = WorldChunker.ChunkToWorldAbsolute( GetWorldChunk( gameObject ) );
		return gameObject.Transform.Position - chunkPos;
	}

	public static Vector2Int GetWorldChunk( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return Vector2Int.Zero;

		var absolutePosition = gameObject.GetAbsolutePosition();
		return WorldChunker.WorldToChunkAbsolute( absolutePosition );
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

	public static void DoDamage( this GameObject obj, DamageInfo damage )
	{
		if ( !obj.Components.TryGet<Health>( out var health, FindMode.EnabledInSelf | FindMode.InAncestors ) )
			return;

		health.OnDamage( damage );
	}

	/// <summary>
	/// Returns a ray using this GameObject's position and forward direction.
	/// </summary>
	public static Ray GetRay( this GameObject obj )
	{
		return new Ray( obj.Transform.Position, obj.Transform.Rotation.Forward );
	}

	public static void ToggleParticleEmission( this GameObject obj, bool shouldEmit )
	{
		var helper = obj.Components.GetOrCreate<ParticleHelper>();
		helper.ToggleEmit( shouldEmit );
	}

	public static void SetParticleTint( this GameObject obj, Color color )
	{
		var helper = obj.Components.GetOrCreate<ParticleHelper>();
		helper.SetTint( color );
	}
	
	public static TagSurface GetTagSurface( this GameObject gameObject )
	{
		var surfaces = ResourceLibrary.GetAll<TagSurface>();
		return surfaces.Where( s => gameObject.Tags.Has( s.ResourceName ) )
			.OrderByDescending( s => s.Priority )
			.FirstOrDefault() 
			?? surfaces.First( s => s.ResourceName == "solid" );
	}

	public static GameObject VisualClone( this GameObject gameObject, GameObject parent, TagSet excludeTags = null, bool cloneLights = false )
	{
		var goClone = new GameObject();
		goClone.Parent = parent;
		goClone.Transform.Local = gameObject.Transform.Local;
		goClone.Name = $"{gameObject.Name} (Clone)";
		
		if ( cloneLights && gameObject.Components.TryGet<PointLight>( out var light ) )
		{
			var lightClone = goClone.Components.Create<PointLight>();
			lightClone.LightColor = light.LightColor;
			lightClone.Attenuation = light.Attenuation;
			lightClone.Radius = light.Radius;
		}
		if ( gameObject.Components.TryGet<ModelRenderer>( out var renderer ) )
		{
			var rendererClone = goClone.Components.Create<ModelRenderer>();
			rendererClone.Model = renderer.Model;
			rendererClone.Tint = renderer.Tint;
			rendererClone.MaterialOverride = renderer.MaterialOverride;
		}
		foreach( var child in gameObject.Children )
		{
			if ( excludeTags is not null && child.Tags.HasAny( excludeTags ) )
				continue;

			VisualClone( child, goClone, excludeTags, cloneLights );
		}
		return goClone;
	}

	public static void RecursiveMaterialOverride( this GameObject gameObject, Material material )
	{
		if ( gameObject.Components.TryGet<ModelRenderer>( out var renderer ) )
		{
			renderer.MaterialOverride = material;
			renderer.SceneObject.Flags.IsTranslucent = true;
		}
		foreach( var child in gameObject.Children )
		{
			RecursiveMaterialOverride( child, material );
		}
	}

	public static void RecursiveTint( this GameObject gameObject, Color tint )
	{
		if ( gameObject.Components.TryGet<ModelRenderer>( out var renderer ) )
		{
			renderer.Tint = tint;
		}
		foreach ( var child in gameObject.Children )
		{
			RecursiveTint( child, tint );
		}
	}

	public static bool IsOnScreen( this GameObject gameObject, float offscreenMargin = 500f )
	{
		if ( Game.ActiveScene is null || !gameObject.IsValid() )
			return false;

		var camera = Game.ActiveScene.Camera;
		var screenFocus = camera.ScreenNormalToWorld( 0.5f );
		var dirToScreenFocus = gameObject.Transform.Position.Direction( screenFocus );
		var cullPoint = gameObject.Transform.Position;
		if ( offscreenMargin > 0f )
		{
			cullPoint += dirToScreenFocus * 500f;
		}
		var screenNormal = camera.PointToScreenNormal( cullPoint );
		return screenNormal.x > 0f && screenNormal.x < 1f && screenNormal.y > 0f && screenNormal.y < 1f;
	}

	public static bool IsPlayer( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return false;

		return gameObject.Components.Get<ShipController>() != null;
	}
}
