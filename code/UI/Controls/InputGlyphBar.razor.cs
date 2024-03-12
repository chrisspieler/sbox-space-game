using System.Collections.Generic;
using System.Linq;
using Sandbox.UI;

[StyleSheet()]
public partial class InputGlyphBar : Panel
{
	public static InputGlyphBar Instance { get; private set; }
	private Dictionary<string, InputGlyphData> Glyphs { get; set; } = new();

	public InputGlyphBar()
	{
		Instance = this;
	}

	public void AddGlyph( InputGlyphData data )
	{
		Glyphs[data.ActionName] = data;
		DeleteChildren( true );
		StateHasChanged();
	}

	public override void Tick()
	{
		var glyphs = Glyphs.Values.ToList();
		foreach ( var glyph in glyphs )
		{
			if ( glyph.RemovalPredicate?.Invoke() == true )
			{
				Glyphs.Remove( glyph.ActionName );
			}
		}
		StateHasChanged();
	}

	private void DrawGlyph( InputGlyphData data )
	{
		var glyphControl = new InputGlyphControl();
		glyphControl.GlyphData = data;
		AddChild( glyphControl );
	}
}
