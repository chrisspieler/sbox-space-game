using Sandbox;
using System;
using System.IO;

public sealed class SaveManager : Component
{
	[ConVar( "save_filename" )]
	public static string ActiveFileName { get; set; }

	[Property] public Scenario DefaultScenario { get; set; }
	[Property] public GameObject ShipPrefab { get; set; }
	[Property] public float AutosaveInterval { get; set; } = 60;

	private TimeUntil _untilNextAutosave;

	protected override void OnAwake()
	{
		if ( TryLoadCareer( ActiveFileName, out var career ) )
		{
			Log.Info( $"Loaded career from file: {FileNameToPath( ActiveFileName )}" );
			Career.Active = career;
		}
		else
		{
			Log.Info( $"No existing save, loading from default scenario: {DefaultScenario.ResourceName}" );
			Career.Active = DefaultScenario.ToCareer();
		}
		ShipController.Respawn();
		_untilNextAutosave = AutosaveInterval;
	}

	protected override void OnUpdate()
	{
		if ( Career.Active is null )
			return;

		Career.Active.TotalPlayTime += Time.Delta;
		if ( _untilNextAutosave )
		{
			_untilNextAutosave = AutosaveInterval;
			SaveActiveCareer();
		}
	}

	[ConCmd("save_manual")]
	public static void SaveActiveCareer()
	{
		if ( Career.Active is null || string.IsNullOrWhiteSpace( ActiveFileName) )
			return;

		SaveCareer( Career.Active, ActiveFileName );
		Log.Info( $"Saved file: {ActiveFileName}" );
	}

	public static void SaveCareer( Career career, string fileName )
	{
		var filePath = FileNameToPath( fileName );
		if ( career is null || string.IsNullOrWhiteSpace( filePath ) )
			return;

		FileSystem.Data.CreateDirectory( "saves" );
		FileSystem.Data.WriteJson( filePath, career );
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

	public static bool TryLoadCareer( string fileName, out Career career )
	{
		var filePath = FileNameToPath( fileName );
		if ( !FileSystem.Data.FileExists( filePath ) )
		{
			career = null;
			return false;
		}

		career = FileSystem.Data.ReadJson<Career>( filePath );
		if ( career is null )
		{
			Log.Error( $"Unable to deserialize save: {fileName}" );
		}
		return career is not null;
	}
}