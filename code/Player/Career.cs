using System;
using Sandbox;

public class Career
{
	public static Career Active { get; set; }

	public int Money
	{
		get => _money;
		set
		{
			_money = Math.Max( value, 0 );
		}
	}
	private int _money;

	[ConCmd("add_money")]
	public static void AddMoney( int money )
	{
		if ( Active is null || money < 0 ) 
			return;

		Active.Money += money;
	}

	[ConCmd("remove_money")]
	public static void RemoveMoney( int money )
	{
		if ( Active is null || money < 0 )
			return;

		Active.Money -= money;
	}

	public static bool HasMoney( int money )
	{
		if ( Active is null )
			return false;

		return Active.Money >= money;
	}
}
