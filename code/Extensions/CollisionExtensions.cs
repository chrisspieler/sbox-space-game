using Sandbox.Utility;

namespace Sandbox;

public static class CollisionExtensions
{
	[ConVar( "collision_damage_debug" )]
	public static bool DebugDamage { get; set; }
	[ConVar( "collision_damage_top_speed" )]
	public static float DamageTopSpeed { get; set; } = 10_000f;
	[ConVar( "collision_damage_max" )]
	public static float DamageMax { get; set; } = 10_000f;
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
		var speedFraction = collision.Contact.Speed.Length.LerpInverse( 0f, DamageTopSpeed );
		if ( collision.Self.GameObject.Tags.Has( "player" ) && ShipController.GetCurrent() is not null )
		{
			// TODO: Make extra sure that the when the player is nearly stopped next to another object, they
			//	can't take very much damage from that object.
		}
		speedFraction = Easing.SineEaseIn( speedFraction );
		var damage = MathX.Lerp( 0f, DamageMax, speedFraction );
		if ( DebugDamage )
		{
			Log.Info( $"{collision.Self.GameObject.Name} vs {collision.Other.GameObject.Name} damage {damage}, contact speed: {collision.Contact.Speed.Length}, normal speed {collision.Contact.NormalSpeed}" );
		}
		return damage;
	}
}
