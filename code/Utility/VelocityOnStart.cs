using Sandbox;

public sealed class VelocityOnStart : Component
{
	[Property, RequireComponent] public Rigidbody Rigidbody { get; set; }
	[Property] public bool RandomizeVelocity { get; set; } = true;
	[Property] public Vector3 VelocityDirection { get; set; }
	[Property] public float VelocityScale { get; set; } = 1f;
	[Property] public bool ZPlaneOnly { get; set; }
	[Property] public bool RandomizeAngularVelocity { get; set; } = true;
	[Property] public Vector3 AngularVelocityDirection { get; set; }
	[Property] public float AngularVelocityScale { get; set; } = 1f;

	protected override void OnStart()
	{
		SetRandomVelocity();
		SetRandomAngularVelocity();
	}

	private void SetRandomVelocity()
	{
		var velocity = VelocityDirection;
		if ( RandomizeVelocity )
		{
			velocity = Vector3.Random;
		}
		if ( ZPlaneOnly )
		{
			velocity = velocity.WithZ( 0f ).Normal;
		}
		velocity *= VelocityScale;
		Rigidbody.Velocity = velocity;
	}

	private void SetRandomAngularVelocity()
	{
		var angularVelocity = AngularVelocityDirection;
		if ( RandomizeAngularVelocity )
		{
			angularVelocity = Vector3.Random;
		}
		angularVelocity *= AngularVelocityScale;
		Rigidbody.AngularVelocity = angularVelocity;
	}
}
