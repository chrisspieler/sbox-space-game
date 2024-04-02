using Sandbox;

public sealed partial class ScreenManager : Component
{
	[Property] public GameObject BeaconContainer { get; set; }

	public static void SetBeaconVisibility( bool isVisible )
	{
		Instance.BeaconContainer.Enabled = isVisible;
	}

	/// <summary>
	/// Adds a <see cref="BeaconPanel"/> to the screen that follows the position or <see cref="GameObject"/>
	/// specified by the given target. 
	/// </summary>
	/// <param name="target">A target whose position and direction will be represented on-screen.</param>
	/// <param name="beaconId">A unique ID that can be used to remove the beacon via <see cref="RemoveBeacon(string)"/>.</param>
	/// <param name="name">A friendly name for the beacon, for display on-screen.</param>
	public static void AddBeacon( Target target, string beaconId, string name )
	{
		// No two beacons may share the same Id.
		if ( GetBeacon( beaconId ) is not null )
		{
			Log.Info( $"Beacon already added: {beaconId}" );
			return;
		}

		var panelGo = new GameObject( true, $"Beacon Panel ({name ?? "null"})" );
		panelGo.Parent = Instance.BeaconContainer;
		var beaconPanel = panelGo.Components.Create<BeaconPanel>();
		beaconPanel.Target = target;
		beaconPanel.BeaconId = beaconId;
		beaconPanel.Name = name;
	}

	/// <summary>
	/// Adds a <see cref="BeaconPanel"/> to the screen that follows the position of the <see cref="GameObject"/>
	/// that the specified <see cref="Beacon"/> is attached to.
	/// </summary>
	public static void AddBeacon( Beacon beacon )
	{
		if ( !beacon.IsValid() )
			return;

		AddBeacon( beacon.GameObject, beacon.BeaconId, beacon.Name );
	}

	/// <summary>
	/// Removes a <see cref="BeaconPanel"/> by <see cref="Beacon.BeaconId"/>. This is the only way a beacon
	/// that was tracking a position rather than a <see cref="GameObject"/> can be removed.
	/// </summary>
	/// <param name="beaconId"></param>
	public static void RemoveBeacon( string beaconId )
	{
		var existing = GetBeacon( beaconId );
		if ( existing is null )
			return;

		existing.GameObject.DestroyImmediate();
	}

	/// <summary>
	/// Removes a <see cref="BeaconPanel"/> that was tracking the position of a <see cref="GameObject"/>
	/// with the specified <see cref="Beacon"/> component.
	/// </summary>
	public static void RemoveBeacon( Beacon beacon )
	{
		if ( !beacon.IsValid() )
		{
			Log.Info( "Beacon invalid" );
			return;
		}

		var existing = GetBeacon( beacon );
		if ( existing is null )
			return;

		existing.GameObject.DestroyImmediate();
	}

	/// <summary>
	/// Returns the <see cref="BeaconPanel"/> that corresponds to the given <paramref name="id"/>, or
	/// <c>null</c> if no such panel is found.
	/// </summary>
	private static BeaconPanel GetBeacon( string id )
	{
		if ( id is null )
			return null;

		foreach ( var child in Instance.BeaconContainer.Children )
		{
			var panel = child.Components.Get<BeaconPanel>();
			if ( panel.IsValid() && panel.BeaconId == id )
			{
				return panel;
			}
		}
		return null;
	}

	private static BeaconPanel GetBeacon( Beacon beacon )
	{
		if ( !beacon.IsValid() )
			return null;

		var foundBeacon = GetBeacon( beacon.BeaconId );
		return foundBeacon.Target.GameObject == beacon.GameObject
			? foundBeacon
			: null;
	}
}
