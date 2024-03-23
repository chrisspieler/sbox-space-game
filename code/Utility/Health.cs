using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Health : Component, Component.IDamageable, IHealth
{
	[Property] public Action<DamageInfo> OnKilled { get; set; }
	[Property] public Action<DamageInfo> OnDamaged { get; set; }
	[Property] public SoundEvent DamageSound { get; set; }

	[Property] public bool IsInvincible { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public Shield Shield { get; set; }
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
		if ( Shield is not null && Shield.CurrentHealth > 0f )
		{
			Shield.OnDamage( damage );
			return;
		}
		OnDamaged?.Invoke( damage );
		if ( Tags.Has( "player" ) )
		{
			var shakeAmount = MathF.Max( 0.1f, damage.Damage / MaxHealth * 2 );
			ScreenEffects.AddScreenShake( shakeAmount );
		}
		var damageAmount = IsInvincible ? 0f : damage.Damage;
		var wasAlive = IsAlive;
		CurrentHealth = Math.Max( 0f, CurrentHealth - damageAmount );
		if ( wasAlive && !IsAlive )
		{
			OnKilled?.Invoke( damage );
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
		if ( DamageSound is not null )
		{
			var hSnd = Sound.Play( DamageSound, Transform.Position );
			var isPlayerInvolved = Tags.Has( "player" ) || damage.Attacker.Tags.Has( "player" );
			// Attacks by and on the player should produce the most noticeable sounds.
			// For everything else (e.g. asteroids hitting each other), turn down the volume.
			if ( !isPlayerInvolved )
			{
				var intensity = damage.Damage.LerpInverse( 0f, MaxHealth / 2f );
				hSnd.Volume = DamageSound.Volume.GetValue() * intensity;
			}
		}
	}
}
