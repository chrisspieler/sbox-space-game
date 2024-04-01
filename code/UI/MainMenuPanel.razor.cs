using Sandbox;

public partial class MainMenuPanel : PanelComponent
{
	[Property] public string Title { get; set; } = "Aluminum Acrobat";
	[Property] public SceneFile MainScene { get; set; }
	[Property] public Career CareerInProgress { get; set; }
	[Property] public Scenario DefaultScenario { get; set; }
	[Property] public PanelComponent SettingsMenu { get; set; }
	[Property] public PanelComponent CreditsMenu { get; set; }
	[Property] public PanelComponent ControlStyleChooser { get; set; }

	private bool IsHidden { get; set; } = true;

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( Title, IsHidden );

	protected override void OnStart()
	{
		base.OnStart();
		_ = Task.DelaySeconds( 1f ).ContinueWith( _ => IsHidden = false );
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Settings.LoadAndApply();
		SaveManager.ActiveFileName = string.Empty;
		// TODO: Find multiple slots.
		CareerInProgress = FindExistingSave( 0 );
	}

	public void StartGame()
	{
		if ( !ControlStyleChoicePanel.HasChosenTankControls )
		{
			((ControlStyleChoicePanel)ControlStyleChooser).OnClose += StartGame;
			ControlStyleChooser.Enabled = true;
			return;
		}
		if ( CareerInProgress is null )
		{
			var normalCareer = DefaultScenario.ToCareer();
			var fileName = SaveManager.GetSaveSlotFileName( 0 );
			SaveManager.SaveCareer( normalCareer, fileName );
		}
		// TODO: Allow for different save slots to be saved and loaded.
		SetActiveSave( 0 );
		Scene.Load( MainScene );
	}

	private Career FindExistingSave( int slot )
	{
		var fileName = SaveManager.GetSaveSlotFileName( 0 );
		Career career;
		SaveManager.TryLoadCareer( fileName, out career );
		return career;
	}

	private void SetActiveSave( int slot )
	{
		SaveManager.ActiveFileName = SaveManager.GetSaveSlotFileName( slot );
	}

	private void Quit()
	{
		Game.Close();
	}

	private void OpenSettingsMenu()
	{
		SettingsMenu.Enabled = true;
		Enabled = false;
	}

	private void OpenCreditsMenu()
	{
		CreditsMenu.Enabled = true;
		Enabled = false;
	}
}
