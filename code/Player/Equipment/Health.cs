using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Health : Component, Component.IDamageable
{
	[Property] public Action OnKilled { get; set; }
	[Property] public Action<DamageInfo> OnDamaged { get; set; }

	[Property] public bool IsInvincible { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public Bouncy Bounce { get; set; }

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
		var damageAmount = IsInvincible ? 0f : damage.Damage;
		CurrentHealth = Math.Max( 0f, CurrentHealth - damageAmount );
		if ( CurrentHealth <= 0f )
		{
			OnKilled?.Invoke();
			return;
		}
		var effect = new TintEffect
		{
			Tint = Color.Red,
			UntilFadeEnd = 1f,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		Body.ColorFlash( effect );
	}
}
