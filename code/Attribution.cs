namespace Sandbox;

[GameResource("Attribution", "attrb", "Credit given to the original author of an asset.", Icon = "menu_book")]
public class Attribution : GameResource
{
	[Property] public string Author { get; set; }
	[Property] public string Name { get; set; }
	[Property] public string Source { get; set; }
	[Property] public string License { get; set; }
	[Property] public string AssetPath { get; set; }
}
