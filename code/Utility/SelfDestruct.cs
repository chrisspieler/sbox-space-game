using Sandbox;


public sealed class SelfDestruct : Component
{
	[Property] public float Delay { get; set; } = 1f;

	private TimeUntil _untilDestroy;

	protected override void OnStart()
	{
		_untilDestroy = Delay;
	}

	protected override void OnUpdate()
	{
		if ( _untilDestroy )
		{
			GameObject.Destroy();
		}
	}
}
