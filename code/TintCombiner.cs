using Sandbox.Utility;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class TintCombiner : Component
{
	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public Color BaseTint { get; set; } = Color.White;
	[Property] public int TintCount => _activeTints.Count;

	private List<TintEffect> _activeTints = new();

	protected override void OnUpdate()
	{
		RemoveExpiredEffects();

		if ( !_activeTints.Any() || !Renderer.IsValid() )
		{
			Renderer.Tint = BaseTint;
			return;
		}

		var firstEffect = _activeTints[0];
		Color firstColor = GetEffectColor( firstEffect );
		Color totalColor = BaseTint.Blend( firstColor, firstEffect.BlendMode );

		foreach( var effect in _activeTints.Skip( 1 ) )
		{
			var currentColor = GetEffectColor( effect );
			totalColor = totalColor.Blend( currentColor, effect.BlendMode );
		}
		Renderer.Tint = totalColor;
	}

	private static Color GetEffectColor( TintEffect effect )
	{
		if ( !effect.UntilFadeEnd.HasValue )
			return effect.Tint;

		var progress = effect.UntilFadeEnd.Value.Fraction;
		if ( effect.EasingFunction != null )
		{
			progress = effect.EasingFunction( progress );
		}
		return Color.Lerp( Color.Transparent, effect.Tint, 1f - progress );
	}

	private void RemoveExpiredEffects()
	{
		foreach( var effect in _activeTints.ToList() )
		{
			if ( !effect.UntilFadeEnd.HasValue )
				continue;

			if ( effect.UntilFadeEnd.Value )
			{
				_activeTints.Remove( effect );
			}
		}
	}

	public int AddTint( Color color, ColorBlendMode blendMode = ColorBlendMode.Normal )
	{
		_activeTints.Add( new TintEffect
		{
			Tint = color,
			BlendMode = blendMode
		} );
		return _activeTints.Count - 1;
	}

	public int AddTint( Color color, TimeUntil untilFadeEnd, ColorBlendMode blendMode = ColorBlendMode.Normal, Easing.Function easingFunction = null )
	{
		_activeTints.Add( new TintEffect
		{
			Tint = color,
			BlendMode = blendMode,
			UntilFadeEnd = untilFadeEnd,
			EasingFunction = easingFunction
		});
		return _activeTints.Count - 1;
	}

	public int AddTint( TintEffect effect )
	{
		if ( _activeTints.Contains( effect ) )
			return -1;

		_activeTints.Add( effect );
		return _activeTints.Count - 1;
	}

	public bool RemoveTint( int index )
	{
		if ( index < 0 || index >= _activeTints.Count )
			return false;
		_activeTints.RemoveAt( index );
		return true;
	}
}
