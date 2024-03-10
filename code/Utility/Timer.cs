using Sandbox;
using System;

public sealed class Timer : Component
{
	[Property] public Action OnDing { get; set; }
	[Property] public float IntervalSeconds { get; set; } = 1f;
	[Property] public bool InvokeOnStart { get; set; } = false;

	private TimeUntil _untilNextInvoke;

	protected override void OnStart()
	{
		_untilNextInvoke = IntervalSeconds;
		if ( InvokeOnStart )
		{
			OnDing?.Invoke();
		}
	}

	protected override void OnUpdate()
	{
		if ( !_untilNextInvoke )
			return;

		_untilNextInvoke = IntervalSeconds;
		OnDing?.Invoke();
	}
}
