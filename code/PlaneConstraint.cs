using Sandbox;

public sealed class PlaneConstraint : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }

	protected override void OnStart()
	{
		Rigidbody ??= Components.Get<Rigidbody>();
	}

	protected override void OnUpdate()
	{
		Transform.Position = Transform.Position.WithZ( 0f );

		if ( Rigidbody is null )
			return;

		Rigidbody.Velocity = Rigidbody.Velocity.WithZ( 0f );
	}
}
