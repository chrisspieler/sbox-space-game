using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public partial class Career
{
	public struct SavedCargoValue
	{
		public string Name { get; set; }
		public int Value { get; set; }

		public CargoValue ToCargoValue()
		{
			var thisName = Name.ToLower();
			var cargo = ResourceLibrary.GetAll<CargoItem>()
				.FirstOrDefault( c => c.ResourceName.ToLower() == thisName );
			if ( cargo is null )
				return null;
			return new CargoValue( cargo, Value );
		}
	}

	public struct RepeatableUpgradeState
	{
		public string Name { get; set; }
		public int Level { get; set; }
	}

	public enum LaserStyle
	{
		Tinted,
		Gradient
	}

	[ConVar("career_respawn_fee")]
	public static int RespawnFee { get; set; } = 100_000;

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

	public List<SavedCargoValue> ShopCargoValues { get; set; } = new();
	public string World { get; set; } = "default";
	public LaserStyle LaserStylePreference { get; set; } = LaserStyle.Tinted;
	public Color LaserTintPreference { get; set; } = Color.Red;
	public string LaserGradientPreference { get; set; } = "rainbow";

	public LaserGradient GetPreferredLaserGradient()
	{
		return ResourceLibrary.GetAll<LaserGradient>()
			.First( g => g.ResourceName.ToLower() == LaserGradientPreference.ToLower() );
	}

	public CargoValues GetCargoValues()
	{
		if ( !ShopCargoValues.Any() )
		{
			var defaultValues = ResourceLibrary.GetAll<CargoItem>()
				.Select( c => new CargoValue( c, c.BaseValue ) );
			UpdateCargoValues( new CargoValues( defaultValues ) );
		}
		var values = ShopCargoValues.Select( v => v.ToCargoValue() );
		return new CargoValues( values );
	}

	public void UpdateCargoValues( CargoValues newValues )
	{
		ShopCargoValues = newValues
			.GetAllValues()
			.Select( v => new SavedCargoValue() { Name = v.Cargo.ResourceName, Value = v.CurrentValue } )
			.ToList();
	}

	public bool HasMoney() => HasMoney( 1 );

	public bool HasMoney( int money ) => Money >= money;

	public void AddMoney( int money )
	{
		if ( money < 0 )
			return;

		if ( !CheatManager.HasCheated && Game.ActiveScene.IsMainGameplayScene() )
		{
			Sandbox.Services.Stats.Increment( "credits-total", money );
		}
		TotalCreditsEarned += money;
		Money += money;
	}
	public void RemoveMoney( int money )
	{
		if ( money < 0 )
			return;

		Money -= money;
		Money = Math.Max( 0, Money );
	}
}
