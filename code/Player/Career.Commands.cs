using Sandbox;

public partial class Career
{
	[ConCmd( "career_status" )]
	public static void PrintStatus()
	{
		Log.Info( $"Active Career: {Active is not null}" );
		Log.Info( $"Career Credits: {Active?.Money ?? 0}" );
		Log.Info( $"Upgrades" );
		Log.Info( $"---" );
		foreach ( var upgrade in Active.Upgrades )
		{
			Log.Info( $"\t{upgrade.Name}" );
		}
	}

	[ConCmd( "add_money" )]
	public static void AddMoneyCommand( int money )
	{
		Active?.AddMoney( money );
	}

	[ConCmd( "remove_money" )]
	public static void RemoveMoneyCommmand( int money )
	{
		Active?.RemoveMoney( money );
	}
}
