using Sandbox;

/// <summary>
/// Applies an initial velocity and angular velocity to the specified <see cref="Rigidbody"/>.
/// </summary>
public sealed class InitialVelocity : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }

	[Property, ToggleGroup( "ApplyInitialVelocity", Label = "Apply Initial Velocity" )]
	public bool ApplyInitialVelocity { get; set; } = true;
	[Property, Group( "ApplyInitialVelocity" )] 
	public bool RandomizeVelocityDirection { get; set; } = true;
	[Property, Group( "ApplyInitialVelocity" ), ShowIf( nameof( RandomizeVelocityDirection ), false )]
	public Vector3 VelocityDirection { get; set; } = Vector3.Forward;
	[Property, Group( "ApplyInitialVelocity" )] 
	public RangedFloat VelocityScale { get; set; } = 20f;

	[Property, ToggleGroup( "ApplyInitialAngularVelocity", Label = "Apply Initial Angular Velocity" )]
	public bool ApplyInitialAngularVelocity { get; set; } = true;
	[Property, Group( "ApplyInitialAngularVelocity" )] 
	public bool RandomizeAngularDirection { get; set; } = true;
	[Property, Group( "ApplyInitialAngularVelocity" ), ShowIf( nameof( RandomizeAngularDirection ), false )]
	public Vector3 AngularDirection { get; set; } = Vector3.Forward;
	[Property, Group( "ApplyInitialAngularVelocity" )] 
	public RangedFloat AngularScale { get; set; } = 5f;

	protected override void OnStart()
	{
		Rigidbody ??= Components.Get<Rigidbody>();
		if ( ApplyInitialVelocity )
		{
			ApplyVelocity();
		}
		if ( ApplyInitialAngularVelocity )
		{
			ApplyAngularVelocity();
		}
	}

	private void ApplyVelocity()
	{
		if ( RandomizeVelocityDirection )
		{
			VelocityDirection = Vector3.Random;
		}
		Rigidbody.Velocity = VelocityDirection.Normal * VelocityScale.GetValue();
	}

	private void ApplyAngularVelocity()
	{
		if ( RandomizeAngularDirection )
		{
			AngularDirection = Vector3.Random;
		}
		Rigidbody.AngularVelocity = AngularDirection.Normal * AngularScale.GetValue();
	}
}
