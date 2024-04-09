using Sandbox;
using Sandbox.UI;

public partial class RadialMenuItem : Panel
{
	public bool IsSelected { get; set; }
	public RadialMenu.Item Item { get; set; }

	public RadialMenuItem() { }

	public override void OnLayout( ref Rect layoutRect )
	{
		base.OnLayout( ref layoutRect );

		var halfWidth = layoutRect.Width / 2f;
		var halfHeight = layoutRect.Height / 2f;

		layoutRect.Left -= halfWidth;
		layoutRect.Top -= halfHeight;
		layoutRect.Right -= halfWidth;
		layoutRect.Bottom -= halfHeight;
	}

	protected override void OnParametersSet()
	{
		BindClass( "selected", () => IsSelected );
		base.OnParametersSet();
	}

	protected override int BuildHash()
	{
		return Item.GetHashCode();
	}
}
