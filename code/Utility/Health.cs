using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Health : Component, Component.IDamageable, IHealth
{
	[Property] public Action OnKilled { get; set; }
	[Property] public Action<DamageInfo> OnDamaged { get; set; }

	[Property] public bool IsInvincible { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public Bouncy Bounce { get; set; }
	[Property] public Color DamageFlashColor { get; set; } = Color.Red;

	public bool IsAlive => CurrentHealth > 0f;

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
		Bounce ??= Components.Get<Bouncy>();
		if ( Bounce.IsValid() )
		{
			Bounce.OnBounce += Hit;
		}
	}

	public void Hit( Collision collision )
	{
		var damageInfo = new DamageInfo()
		{
			Attacker = collision.Other.GameObject,
			Position = collision.Contact.Point,
			Damage = collision.GetDamage()
		};
		OnDamage( damageInfo );
	}

	public void OnDamage( in DamageInfo damage )
	{
		OnDamaged?.Invoke( damage );
		if ( Tags.Has( "player" ) )
		{
			ScreenEffects.AddScreenShake( damage.Damage / MaxHealth * 2 );
		}
		var damageAmount = IsInvincible ? 0f : damage.Damage;
		var wasAlive = IsAlive;
		CurrentHealth = Math.Max( 0f, CurrentHealth - damageAmount );
		if ( wasAlive && !IsAlive )
		{
			OnKilled?.Invoke();
			return;
		}
		var effect = new TintEffect
		{
			Tint = DamageFlashColor,
			UntilFadeEnd = 1f,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		Body.ColorFlash( effect );
	}
}
