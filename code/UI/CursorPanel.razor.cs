using Sandbox;
using Sandbox.UI;

public partial class CursorPanel : PanelComponent
{
	[Property] public string CursorImagePath { get; set; } = "ui/img/crosshair.svg";

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( CursorImagePath );

	protected override void OnUpdate()
	{
		var mouseNormalPos = Mouse.Position / Screen.Size;
		if ( mouseNormalPos.x < 0 || mouseNormalPos.x > 1 || mouseNormalPos.y < 0 || mouseNormalPos.y > 1 )
			return;
		Panel.Style.Left = Length.Percent( mouseNormalPos.x * 100f );
		Panel.Style.Top = Length.Percent( mouseNormalPos.y * 100f );
		StateHasChanged();
	}
}
