using Sandbox;

public sealed class Beacon : Component, IWorldStreamingListener
{
	[Property] public string Name { get; set; }
	/// <summary>
	/// A unique ID for this beacon. Used by <see cref="ScreenManager"/>.
	/// </summary>
	[Property] public string BeaconId { get; set; }

	protected override void OnEnabled()
	{
		// This beacon might already be on screen as a distant, position-targeting BeaconPanel.
		// Now that this beacon has been streamed in as part of a GameObject, we should remove
		// the existing BeaconPanel and replace it with a GameObject-targeting BeaconPanel.
		ScreenManager.RemoveBeacon( BeaconId );
		ScreenManager.AddBeacon( this );
	}

	protected override void OnDisabled()
	{
		ScreenManager.RemoveBeacon( this );
	}

	public static Beacon Create( Vector3 relativePosition, string name = "New Beacon", float destroyAfterSeconds = 0f )
	{
		var beaconGo = new GameObject( true, name );
		beaconGo.Transform.Position = relativePosition.WithZ( 0f );
		var beacon = beaconGo.Components.Create<Beacon>();
		beacon.Name = name;
		if ( destroyAfterSeconds > 0f )
		{
			var selfDestruct = beaconGo.Components.Create<SelfDestruct>();
			selfDestruct.Delay = destroyAfterSeconds;
		}
		return beacon;
	}

	public void OnWorldUnload( bool wholeChunkUnloaded )
	{
		ScreenManager.RemoveBeacon( this );
		ScreenManager.AddBeacon( GameObject.Transform.Position, BeaconId, Name );
	}
}
