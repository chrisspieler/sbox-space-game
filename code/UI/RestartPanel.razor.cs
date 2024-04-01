using Sandbox;

public partial class RestartPanel : PanelComponent
{
	[Property] public string DeathText { get; set; } = "CLONE DEAD";
	[Property] public string RespawnFeeLabel { get; set; } = "YOU HAVE LOST";
	// TODO: Use a glyph instead of text.
	[Property] public string InstructionText { get; set; } = "Press R to Respawn";

	private string RespawnFeeCost => "ALL OF YOUR CREDITS";

	private bool ShowDeathText { get; set; } = false;
	private bool ShowInstruction { get; set; } = false;

	protected override void OnStart()
	{
		_ = Task.DelaySeconds( 3f ).ContinueWith( _ => ShowDeathText = true );
		_ = Task.DelaySeconds( 3.5f ).ContinueWith( _ => ShowInstruction = true );
	}

	protected override int BuildHash()
		=> System.HashCode.Combine( DeathText, InstructionText, ShowDeathText, ShowInstruction, RespawnFeeLabel );

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "stabilizer" ) )
		{
			ShipController.Respawn();
			Enabled = false;
		}
	}
}
