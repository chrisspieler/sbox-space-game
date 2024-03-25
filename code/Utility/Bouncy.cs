using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class Bouncy : Component, Component.ICollisionListener
{
	[Property] public Action<Collision> OnBounce { get; set; }

	[Property] public float BounceFactor { get; set; } = 0f;
	[Property] public TagSet BouncedTags { get; set; }
	[Property] public TagSet IgnoredTags { get; set; }
	[Property] public float AddedVelocity { get; set; } = 0f;
	[Property] public float RehitDelay { get; set; } = 0.5f;
	[Property] public float MinimumVelocity { get; set; } = 50f;

	private readonly Dictionary<GameObject, TimeUntil> _recentlyHit = new();

	protected override void OnUpdate()
	{
		foreach( var (go, untilExpire) in _recentlyHit.ToList() )
		{
			if ( untilExpire )
			{
				_recentlyHit.Remove( go );
			}
		}
	}

	public void OnCollisionStart( Collision other )
	{
		if ( !Active || BounceFactor <= 0f )
			return;

		if ( !CanBounceFrom( other.Other.GameObject ) )
			return;

		var totalVelocity = other.Other.Body.Velocity.Distance( other.Self.Body.Velocity );
		if ( totalVelocity < MinimumVelocity )
			return;

		_recentlyHit[other.Other.GameObject] = RehitDelay;
		OnBounce?.Invoke( other );
		other.Pongify( BounceFactor );
		other.Other.Body.Velocity *= Vector3.One * AddedVelocity;
	}
	public void OnCollisionStop( CollisionStop other ) { }
	public void OnCollisionUpdate( Collision other ) { }

	private bool CanBounceFrom( GameObject other )
	{
		if ( !other.IsValid() || _recentlyHit.ContainsKey( other ) )
			return false;

		return (BouncedTags is null || BouncedTags.IsEmpty || other.Tags.HasAny( BouncedTags ))
			&& (IgnoredTags is null || IgnoredTags.IsEmpty || !other.Tags.HasAny( IgnoredTags ));
	}
}
