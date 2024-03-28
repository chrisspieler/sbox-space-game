using Sandbox;
using Sandbox.Utility;
using System;
using System.Threading.Tasks;

public sealed class Shield : Component, Component.IDamageable, IHealth
{
	[Property] public Action<DamageInfo> OnDamaged { get; set; }

	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public float RegenRate { get; set; } = 20f;
	[Property] public float RegenDelay { get; set; } = 1f;
	[Property] public ShipController Controller { get; set; }
	[Property] public Collider Collider { get; set; }
	[Property] public ModelRenderer Renderer { get; set; }
	[Property] public Bouncy Bounce { get; set; }
	[Property] public SoundEvent HitSound { get; set; }
	[Property] public SoundEvent BreakSound { get; set; }

	private TimeUntil _regenStart;
	private SceneLight _hitLight;

	protected override void OnEnabled()
	{
		Controller ??= Components.GetInAncestorsOrSelf<ShipController>();
		Collider ??= Components.Get<Collider>( true );
		Bounce ??= Components.Get<Bouncy>( true );
		Renderer ??= Components.Get<ModelRenderer>( true );
		Renderer.Enabled = true;
		if ( Bounce.IsValid() )
		{
			Bounce.OnBounce += Hit;
		}
	}

	protected override void OnDisabled()
	{
		Renderer.Enabled = false;
	}

	protected override void OnUpdate()
	{
		// Workaround https://github.com/Facepunch/sbox-issues/issues/5088
		Renderer.SceneObject.Flags.IsTranslucent = true;
		Renderer.SceneObject.Flags.IsOpaque = false;
		Collider?.SetEnabled( CurrentHealth > 0f );
		Bounce.Enabled = CurrentHealth > 0f;
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
		if ( !Active || CurrentHealth <= 0f )
			return;

		var damageAmount = (Controller.IsValid() && Controller.IsInvincible)
			? 0f
			: damage.Damage;
		CurrentHealth = Math.Max( 0f, CurrentHealth - damageAmount );
		ResetRegen();
		var fadeTime = 0.8f;
		var effect = new TintEffect
		{
			Tint = Color.Cyan.WithAlpha( 0.05f ),
			UntilFadeEnd = fadeTime,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		GameObject.ColorFlash( effect );
		CreateHitLight( Color.Cyan, fadeTime );
		var hitSound = CurrentHealth > 0f
			? HitSound
			: BreakSound;
		if ( hitSound is not null )
		{
			Sound.Play( hitSound, Transform.Position );
		}
	}

	private void CreateHitLight( Color color, float time )
	{
		_hitLight?.Delete();
		_hitLight = new( Scene.SceneWorld )
		{
			Position = Transform.Position,
			ShadowsEnabled = false,
			LightColor = color,
			Radius = 2000f
		};
		_ = FadeOutLight( _hitLight, time );
	}

	private async Task FadeOutLight( SceneLight light, float seconds )
	{
		var initialColor = light.LightColor;
		var endColor = Color.Black;
		TimeUntil untilDestroyLight = seconds;
		while( !untilDestroyLight )
		{
			if ( !light.IsValid() )
			{
				break;
			}
			var eased = Easing.EaseOut( untilDestroyLight.Fraction );
			light.LightColor = Color.Lerp( initialColor, endColor, eased );
			light.Position = Transform.Position;
			await Task.Frame();
		}

		if ( light.IsValid() )
			light.Delete();

		if ( _hitLight == light )
			_hitLight = null;
	}
}
