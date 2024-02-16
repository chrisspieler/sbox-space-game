using Sandbox;
using System;

public sealed class Shield : Component, Component.ICollisionListener
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public float CurrentHealth { get; set; }
	[Property] public float RegenRate { get; set; } = 20f;
	[Property] public float RegenDelay { get; set; } = 1f;
	private TimeUntil _regenStart;
	[Property, Category("Bounce")] public float BounceFactor { get; set; } = 1f;
	[Property, Category("Bounce")] public TagSet BouncedTags { get; set; }


	protected override void OnEnabled()
	{
		GameObject.Tags.Add( "bouncer" );
	}

	protected override void OnUpdate()
	{
		UpdateRegen();
	}

	private void UpdateRegen()
	{
		if ( !_regenStart || CurrentHealth >= MaxHealth )
			return;

		CurrentHealth = MathF.Min( MaxHealth, CurrentHealth + RegenRate * Time.Delta );
	}

	public void Hit( float damage )
	{
		CurrentHealth = Math.Max( 0f, CurrentHealth - damage );
		_regenStart = RegenDelay;
	}

	public void OnCollisionStart( Collision other )
	{
		if ( !CanBounceFrom( other.Other.GameObject ) )
			return;

		other.Pongify( BounceFactor );
		Hit( GetBounceDamage( other ) );
	}

	private bool CanBounceFrom( GameObject other )
	{
		if ( BouncedTags is null || BouncedTags.IsEmpty )
			return true;

		var hasBouncedTag = false;
		foreach ( var tag in BouncedTags.TryGetAll() )
		{
			if ( other.Tags.Has( tag ) )
			{
				hasBouncedTag = true;
				break;
			}
		}
		return hasBouncedTag;
	}

	private float GetBounceDamage( Collision other )
	{
		var speed = other.Contact.Speed.Length;
		return MathX.Remap( speed, 0f, 5000f, 1f, 1000f );
	}

	public void OnCollisionStop( CollisionStop other ) { }
	public void OnCollisionUpdate( Collision other ) { }
}
