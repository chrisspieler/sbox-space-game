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

	public static Vector2Int GetWorldChunk( this GameObject gameObject )
	{
		if ( !gameObject.IsValid() )
			return Vector2Int.Zero;

		var absolutePosition = gameObject.GetAbsolutePosition();
		absolutePosition += 1f;
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
		var shield = obj.Components.GetInDescendantsOrSelf<Shield>();
		if ( shield is not null && shield.CurrentHealth > 0f )
		{
			shield.OnDamage( damage );
			return;
		}
		if ( !obj.Components.TryGet<Health>( out var health, FindMode.EnabledInSelfAndDescendants ) )
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
	
	public static TagSurface GetTagSurface( this GameObject gameObject )
	{
		var surfaces = ResourceLibrary.GetAll<TagSurface>();
		return surfaces.Where( s => gameObject.Tags.Has( s.ResourceName ) )
			.OrderByDescending( s => s.Priority )
			.FirstOrDefault() 
			?? surfaces.First( s => s.ResourceName == "solid" );
	}
}
