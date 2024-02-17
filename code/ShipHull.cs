using Sandbox;
using Sandbox.Utility;
using System;

public sealed class ShipHull : Component
{
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public TagSet HitTags { get; set; }
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
		var damage = collision.GetDamage();
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		if ( CurrentHealth <= 0f )
		{
			Log.Info( "TODO: Make the ship explode" );
			return;
		}
		var effect = new TintEffect
		{
			Tint = Color.Red,
			UntilFadeEnd = 1f,
			BlendMode = ColorBlendMode.Normal,
			EasingFunction = Easing.GetFunction( "ease-out" )
		};
		PartsContainer.ColorFlash( effect );
	}
}
