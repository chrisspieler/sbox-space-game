using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.IO;

public partial class CreditsPanel : PanelComponent
{
	[Property] public PanelComponent ReturnMenu { get; set; }

	private List<Attribution> _attributions = new();
	private Dictionary<string, List<Attribution>> _categories = new();

	protected override void OnStart()
	{
		base.OnStart();

		_attributions.AddRange( GetAttributions() );
		_categories = CategorizeAttributions( _attributions );
	}

	private static Dictionary<string, List<Attribution>> CategorizeAttributions( IEnumerable<Attribution> attributions )
	{
		var categories = new Dictionary<string, List<Attribution>>();
		foreach ( var attribution in attributions )
		{
			var directory = Path.GetDirectoryName( attribution.ResourcePath );
			var category = Path.GetFileName( directory );
			if ( !categories.ContainsKey( category ) )
			{
				categories[category] = new List<Attribution>();
			}
			categories[category].Add( attribution );
		}
		return categories;
	}

	private static IEnumerable<Attribution> GetAttributions()
	{
		return ResourceLibrary.GetAll<Attribution>();
	}

	public void CopyToClipboard( string text )
	{
		Clipboard.SetText( text );
	}

	public void Back()
	{
		SetClass( "hidden", true );
		_ = Task.DelayRealtimeSeconds( 0.15f ).ContinueWith( _ => Enabled = false );
		ReturnMenu.Enabled = true;
	}
}
