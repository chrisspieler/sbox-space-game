namespace Sandbox;

public static class CollisionExtensions
{
	/// <summary>
	/// Sets the velocity of the "self" body of the given collision to be 
	/// reflected across the normal of the contact surface, "other".
	/// </summary>
	public static void Pongify( this Collision collision, float bounceFactor = 1f )
	{
		var selfBody = collision.Self.Body;
		var normal = collision.Contact.Normal;
		// Reflect the velocity of the "self" body across the normal of the contact surface.
		var reflectedVelocity = selfBody.Velocity - 2 * selfBody.Velocity.Dot( normal ) * normal;
		reflectedVelocity = reflectedVelocity.WithZ( 0f );
		selfBody.Velocity = reflectedVelocity * bounceFactor;
	}
}
