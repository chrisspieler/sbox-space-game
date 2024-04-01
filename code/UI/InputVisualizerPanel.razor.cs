using Sandbox;

public partial class InputVisualizerPanel : PanelComponent
{
	private string WKeyClass => _wKeyDown ? "pressed" : "";
	private string AKeyClass => _aKeyDown ? "pressed" : "";
	private string SKeyClass => _sKeyDown ? "pressed" : "";
	private string DKeyClass => _dKeyDown ? "pressed" : "";
	private string VerticalClass => ShipController.GetCurrent()?.UseTankControls == true ? "disabled" : "";

	private bool _wKeyDown;
	private bool _aKeyDown;
	private bool _sKeyDown;
	private bool _dKeyDown;

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( WKeyClass, AKeyClass, SKeyClass, DKeyClass, VerticalClass );

	protected override void OnUpdate()
	{
		_wKeyDown = Input.Down( "forward" );
		_aKeyDown = Input.Down( "left" );
		_sKeyDown = Input.Down( "backward" );
		_dKeyDown = Input.Down( "right" );
	}
}
