using System;

using Sandbox;

public sealed class AsteroidHighlight : Component
{
	[ConVar( "asteroid_highlight_strength" )]
	public static float GlobalStrength { get; set; } = 0f;

	[Property, Range( 0f, 1f, 0.05f )]
	public float OutlineStrength { get; set; } = 1f;
	[Property, RequireComponent] 
	public HighlightOutline Outline { get; set; }

	protected override void OnUpdate()
	{
		if ( GlobalStrength == 0f || !GameObject.IsOnScreen( 1000f ) )
		{
			Outline.Enabled = false;
			return;
		}
		Outline.Enabled = true;
		Outline.Color = Color.White.WithAlpha( 0.02f * OutlineStrength * GlobalStrength );
		Outline.Width = 0.5f;
		Outline.ObscuredColor		= Color.Transparent;
		Outline.InsideColor			= Color.Transparent;
		Outline.InsideObscuredColor	= Color.Transparent;
	}
}
