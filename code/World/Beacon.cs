using Sandbox;

public sealed class Beacon : Component
{
	[Property] public string Name { get; set; }

	private FloatingOriginSystem _originSystem;

	protected override void OnEnabled()
	{
		_originSystem = Scene.GetSystem<FloatingOriginSystem>();
		ScreenManager.AddBeacon( this );
	}

	protected override void OnDisabled()
	{
		ScreenManager.RemoveBeacon( this );
	}

	public float GetMetersFromOrigin()
	{
		var originObject = _originSystem.Origin.IsValid()
			? _originSystem.Origin.GameObject
			: Scene.Camera.GameObject; // If the player is dead, get distance from camera instead.
		var distance = GameObject.GetAbsolutePosition().Distance( originObject.GetAbsolutePosition() );
		return MathX.InchToMeter( distance );
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
}
