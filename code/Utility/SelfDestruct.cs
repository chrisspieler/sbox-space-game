using Sandbox;


public sealed class SelfDestruct : Component, IDestructionListener
{
	[Property] public RangedFloat Delay { get; set; } = 1f;
	[Property] public bool EnableOnMakeDebris { get; set; } = false;

	private TimeUntil _untilDestroy;

	protected override void OnEnabled()
	{
		_untilDestroy = Delay.GetValue();
	}

	protected override void OnUpdate()
	{
		if ( _untilDestroy )
		{
			GameObject.Destroy();
		}
	}

	public void OnMakeDebris()
	{
		if ( EnableOnMakeDebris )
		{
			Enabled = true;
		}
	}
}
