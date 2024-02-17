using System;

namespace Sandbox;

public enum ColorBlendMode
{
	Normal,
	Multiply
}
public static class ColorExtensions
{
	public static Color Clamp( this Color color, float min = 0f, float max = 1f )
	{
		var result = color;
		result.r = Math.Clamp( result.r, min, max );
		result.g = Math.Clamp( result.g, min, max );
		result.b = Math.Clamp( result.b, min, max );
		result.a = Math.Clamp( result.a, min, max );
		return result;
	}

	public static Color Blend( this Color a, Color b, ColorBlendMode blend )
	{
		switch ( blend )
		{
			case ColorBlendMode.Multiply:
				return (a * b).Clamp();
			default:
				return Color.Lerp( a, b.WithAlpha( 1f ), b.a );
		}
	}
}
