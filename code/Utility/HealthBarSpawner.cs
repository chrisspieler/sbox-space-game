using Sandbox;

public sealed class HealthBarSpawner : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public Component HealthSource 
	{
		get => _healthSource;
		set
		{
			if ( value is not IHealth )
				return;

			_healthSource = value;
		}
	}
	private Component _healthSource;

	private bool _recentlyDamaged;
	private TimeUntil _untilHideHealthBar;

	protected override void OnStart()
	{
		HealthSource ??= Components.Get<IHealth>() as Component;
		(HealthSource as IHealth).OnDamaged += HandleDamage;
	}

	private void HandleDamage( DamageInfo damage )
	{
		if ( damage.Damage <= 0f )
			return;

		_recentlyDamaged = true;
		_untilHideHealthBar = 3f;
		ScreenManager.ShowHealthBar( HealthSource as IHealth, Target );
	}

	protected override void OnUpdate()
	{
		if ( _recentlyDamaged )
		{
			if ( _untilHideHealthBar || Target.GetDistanceFromScreenCenter() > 2000f )
			{
				ScreenManager.RemoveHealthBar( HealthSource as IHealth );
				_recentlyDamaged = false;
				return;
			}
		}
	}
}
