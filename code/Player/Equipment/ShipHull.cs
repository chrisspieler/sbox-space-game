using Sandbox;
using Sandbox.Utility;
using System;

public sealed class ShipHull : Component
{
	[Property] public ShipController Controller { get; set; }
	[Property] public GameObject PartsContainer { get; set; }
	[Property] public TagSet HitTags { get; set; }
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public Bouncy Bounce { get; set; }

	protected override void OnStart()
	{
		CurrentHealth = MaxHealth;
		Controller ??= Components.GetInAncestorsOrSelf<ShipController>();
		Bounce ??= Components.Get<Bouncy>();
		if ( Bounce.IsValid() )
		{
			Bounce.OnBounce += Hit;
		}
	}

	public void Hit( Collision collision )
	{
		var damage = Controller.IsInvincible ? 0f : collision.GetDamage();
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		if ( CurrentHealth <= 0f )
		{
			var controller = Components.GetInAncestorsOrSelf<ShipController>();
			controller.Explode();
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
