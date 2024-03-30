namespace Sandbox;

/// <summary>
/// Continuously applies a mass override to a <see cref="Sandbox.Rigidbody"/> that scales 
/// with the worldspace scale of the <see cref="Sandbox.GameObject.Transform"/>.
/// </summary>
public sealed class ScaledMassOverride : Component
{
	/// <summary>
	/// The rigidbody that shall be scaled.
	/// </summary>
	[Property] public Rigidbody Rigidbody { get; set; }
	/// <summary>
	/// The amount that each unit of worldspace scale contributes to the mass override.
	/// </summary>
	[Property] public float MassPerScale { get; set; } = 50f;
	/// <summary>
	/// A base level of mass that is always added to the mass override regardless of scale.
	/// </summary>
	[Property] public float BaseMass { get; set; } = 0f;

	protected override void OnStart()
	{
		Rigidbody ??= Components.Get<Rigidbody>();
	}

	protected override void OnFixedUpdate()
	{
		if ( !Rigidbody.IsValid() )
			return;

		if ( Rigidbody.PhysicsBody is not null )
		{
			Rigidbody.PhysicsBody.Mass = BaseMass + MassPerScale * Transform.Scale.Length;
		}
	}

	/// <summary>
	/// Adds a <see cref="ScaledMassOverride"/> component to the specified <see cref="Sandbox.GameObject"/>.
	/// </summary>
	public static void Apply( GameObject go, float massPerScale, float baseMass = 0f )
	{
		if ( !go.IsValid() )
			return;

		var massOverride = go.Components.GetOrCreate<ScaledMassOverride>();
		massOverride.MassPerScale = massPerScale;
		massOverride.BaseMass = baseMass;
	}
}
