using Sandbox;
using System;
using System.IO;

public sealed class SaveManager : Component
{
	[ConVar( "save_filename" )]
	public static string ActiveFileName { get; set; }

	[Property] public Scenario DefaultScenario { get; set; }
	[Property] public GameObject ShipPrefab { get; set; }

	protected override void OnStart()
	{
		if ( TryLoadCareer( ActiveFileName, out var career ) )
		{
			Log.Info( $"Loaded career from file: {FileNameToPath( ActiveFileName )}" );
			Career.Active = career;
		}
		else
		{
			Log.Info( $"No existing save, loading from default scenario: {DefaultScenario.ResourceName}" );
			Career.Active = GetScenarioFromCareer( DefaultScenario );
		}
		ShipController.Respawn();
	}

	public static string GetSaveSlotFileName( int slot )
	{
		if ( slot < 0 )
		{
			throw new ArgumentOutOfRangeException( nameof(slot) );
		}
		return $"slot{slot}";
	}

	public static string FileNameToPath( string fileName )
	{
		// Trim directory.
		fileName = Path.GetFileName( fileName );
		// Trim extension.
		fileName = Path.GetFileNameWithoutExtension( fileName );
		return $"saves/{fileName}.sav";
	}

	private static bool TryLoadCareer( string fileName, out Career career )
	{
		if ( !FileSystem.Data.FileExists( FileNameToPath( fileName ) ) )
		{
			career = null;
			return false;
		}

		career = FileSystem.Data.ReadJson<Career>( fileName );
		return true;
	}

	private static Career GetScenarioFromCareer( Scenario scenario )
	{
		var career = new Career()
		{
			Money = scenario.Money
		};
		foreach( var upgrade in scenario.Upgrades )
		{
			career.AddUpgrade( upgrade );
		}
		return career;
	}
}
