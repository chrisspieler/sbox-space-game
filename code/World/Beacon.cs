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
}
