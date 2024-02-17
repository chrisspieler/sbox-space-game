using Sandbox;
using Sandbox.Utility;
using System;

public sealed class Shield : Component, Component.ICollisionListener
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public float RegenRate { get; set; } = 20f;
	[Property] public float RegenDelay { get; set; } = 1f;
	private TimeUntil _regenStart;
	[Property] public TintCombiner Tinter { get; set; }
	[Property] public Collider Collider { get; set; }
	[Property, Category("Bounce")] public float BounceFactor { get; set; } = 1f;
	[Property, Category("Bounce")] public TagSet BouncedTags { get; set; }


	protected override void OnEnabled()
	{
		GameObject.Tags.Add( "bouncer" );
		Tinter ??= Components.Get<TintCombiner>();
		Collider ??= Components.Get<Collider>( true );
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

	public void Hit( float damage )
	{
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		ResetRegen();
		Tinter.AddTint( Color.Cyan.WithAlpha( 0.5f ), 0.5f, ColorBlendMode.Normal, Easing.GetFunction( "ease-out" ) );
	}

	public void OnCollisionStart( Collision other )
	{
		if ( !CanBounceFrom( other.Other.GameObject ) )
			return;

		other.Pongify( BounceFactor );
		Hit( other.GetDamage() );
	}

	private bool CanBounceFrom( GameObject other )
	{
		return BouncedTags is null
			|| BouncedTags.IsEmpty
			|| other.Tags.HasAny( BouncedTags );
	}

	public void OnCollisionStop( CollisionStop other ) { }
	public void OnCollisionUpdate( Collision other ) { }
}
