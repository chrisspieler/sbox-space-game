namespace Sandbox;

public static class CollisionExtensions
{
	[ConVar( "collision_damage_top_speed" )]
	public static float DamageTopSpeed { get; set; } = 7_000f;
	[ConVar( "collision_damage_max" )]
	public static float DamageMax { get; set; } = 1_000f;
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

	public static float GetDamage( this Collision collision )
	{
		var speed = collision.Contact.Speed.Length;
		return MathX.Remap( speed, 0f, DamageTopSpeed, 1f, DamageMax );
	}
}
