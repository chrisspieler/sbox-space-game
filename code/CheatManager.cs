using Sandbox;

public class CheatManager : GameObjectSystem
{
	[ConVar( "show_cheat_message" )]
	public static bool ShowCheatMessage { get; set; } = true;

	public static bool HasCheated { get; private set; }

	public CheatManager( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, OnUpdate, "Cheat Manager" );
	}

	private void OnUpdate()
	{
		if ( !ShowCheatMessage || !HasCheated )
			return;

		Gizmo.Draw.Color = Color.Yellow;
		var position = new Vector2( Screen.Width / 2, 5f );
		Gizmo.Draw.ScreenText( "CHEAT MODE ACTIVATED", position, "Poppins", 24, TextFlag.CenterTop );
		Gizmo.Draw.Color = Color.White;
		position += Vector2.Up * 28f;
		Gizmo.Draw.ScreenText( "Stats and leaderboards will be disabled until the game is restarted.", position, "Poppins", 24, TextFlag.CenterTop );
		position += Vector2.Up * 28f;
		Gizmo.Draw.ScreenText( "ConVar to hide this message: show_cheat_message 0", position, "Consolas", 18, TextFlag.CenterTop );
	}

	/// <summary>
	/// This should be called whenever a ConCmd is run that might compromise 
	/// the competitive integrity of the leaderboards.
	/// </summary>
	[ConCmd( "cheat_report" )]
	public static void ReportCheat()
	{
		HasCheated = true;
	}

	/// <summary>
	/// This is meant to be invoked only through codegen via <see cref="CheatAttribute"/>. Call 
	/// <see cref="ReportCheat()"/> instead, or consider adding <see cref="CheatAttribute"/> to 
	/// whatever static method or property ought to be treated like a cheat.
	/// </summary>
	public static void ReportCheat( WrappedMethod method, object _ )
	{
		ReportCheat();
		method.Resume();
	}

	/// <inheritdoc cref="ReportCheat(WrappedMethod, object)"/>
	public static void ReportCheat( WrappedMethod method )
	{
		ReportCheat();
		method.Resume();
	}

	/// <inheritdoc cref="ReportCheat(WrappedMethod, object)"/>
	public static void ReportCheat<T>( WrappedPropertySet<T> property )
	{
		ReportCheat();
	}
}
