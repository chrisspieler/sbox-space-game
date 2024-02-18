using Sandbox;
using Sandbox.Utility;
using System;
using System.Threading.Tasks;

public sealed class Shield : Component
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public float RegenRate { get; set; } = 20f;
	[Property] public float RegenDelay { get; set; } = 1f;
	[Property] public Collider Collider { get; set; }
	[Property] public Bouncy Bounce { get; set; }

	private TimeUntil _regenStart;
	private SceneLight _hitLight;

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
		var fadeTime = 0.8f;
		var effect = new TintEffect
		{
			Tint = Color.Cyan.WithAlpha( 0.3f ),
			UntilFadeEnd = fadeTime,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		GameObject.ColorFlash( effect );
		CreateHitLight( Color.Cyan, fadeTime );
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
