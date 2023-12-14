using Sandbox;

public sealed class Bouncer : Component, Component.ICollisionListener
{
	[Property] public float BounceFactor { get; set; } = 1f;

	protected override void OnEnabled()
	{
		GameObject.Tags.Add( "bouncer" );
	}

	public void OnCollisionStart( Collision other )
	{
		// Bounce the other object, like pong
		var otherBody = other.Other.Body;
		var normal = other.Contact.Normal;
		// Reflect the velocity of the other normal across the normal
		var reflectedVelocity = otherBody.Velocity - 2 * otherBody.Velocity.Dot( normal ) * normal;
		reflectedVelocity = reflectedVelocity.WithZ( 0f );
		otherBody.Velocity = reflectedVelocity * BounceFactor;
	}

	public void OnCollisionStop( CollisionStop other )
	{

	}

	public void OnCollisionUpdate( Collision other )
	{

	}
}
