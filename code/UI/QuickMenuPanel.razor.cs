using Sandbox;

public partial class QuickMenuPanel : PanelComponent
{
	public RadialMenu Menu { get; set; }

	protected override void OnUpdate()
	{
		Menu?.BuildInput();
	}
}
