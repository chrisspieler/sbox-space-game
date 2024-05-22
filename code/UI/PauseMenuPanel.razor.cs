using System.Linq;
using Sandbox;
using Sandbox.UI;

public partial class PauseMenuPanel : PanelComponent
{
	public static float UnpauseTimeScale { get; set; } = 1f;

	[Property] public PanelComponent SettingsPanel { get; set; }
	[Property] public SceneFile MainMenuScene { get; set; }

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( 1 );

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Scene.TimeScale = 0f;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Input.EscapePressed )
		{
			Input.EscapePressed = false;
			Resume();
		}
	}

	public bool CanShowPauseMenu()
	{
		return !ScreenManager.IsPaused() && !SettingsPanel.Enabled && !ScreenManager.IsShopOpen();
	}

	private void Resume()
	{
		Scene.TimeScale = UnpauseTimeScale;
		Enabled = false;
	}

	private void OpenSettings()
	{
		SettingsPanel.Enabled = true;
		Enabled = false;
	}

	private void Quit()
	{
		SaveManager.SaveActiveCareer();
		Game.Close();
		// TODO: Fix the issue with the world chunker that makes going back to the main menu not work.
		// Scene.Load(MainMenuScene);
	}
}
