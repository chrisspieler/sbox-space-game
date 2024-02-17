using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Shield : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public float RegenRate { get; set; } = 20f;
	[Property] public float RegenDelay { get; set; } = 1f;
	[Property] public Collider Collider { get; set; }
	[Property] public Bouncy Bounce { get; set; }

	private TimeUntil _regenStart;

	protected override void OnEnabled()
	{
		Collider ??= Components.Get<Collider>( true );
		Bounce ??= Components.Get<Bouncy>( );
		if ( Bounce.IsValid() )
		{
			Bounce.OnBounce += Hit;
		}
	}

	protected override void OnUpdate()
	{
		Collider.Enabled = CurrentHealth > 0f;
		UpdateRegen();
	}

	private void UpdateRegen()
	{
		if ( !_regenStart || CurrentHealth >= MaxHealth )
			return;

		CurrentHealth = MathF.Min( MaxHealth, CurrentHealth + RegenRate * Time.Delta );
	}

	public void ResetRegen()
	{
		_regenStart = RegenDelay;
	}

	public void Hit( Collision collision )
	{
		var damage = collision.GetDamage();
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		ResetRegen();
		var effect = new TintEffect
		{
			Tint = Color.Cyan.WithAlpha( 0.5f ),
			UntilFadeEnd = 0.5f,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		GameObject.ColorFlash( effect );
	}
}
