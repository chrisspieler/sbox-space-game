using Sandbox;

public sealed class FloatingOriginPlayer : Component
{
	[Property] public float WorldResetDistance { get; set; } = 500f;
	public Vector3 TotalOriginShift { get; private set; }

	protected override void OnStart()
	{
		var floatingOriginSystem = Scene.GetSystem<FloatingOriginSystem>();
		floatingOriginSystem.OnWorldReset += diff => TotalOriginShift -= diff;
	}
}
