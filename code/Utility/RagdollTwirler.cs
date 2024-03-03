using Sandbox;
using System;

public sealed class RagdollTwirler : Component
{
	[Property] public int Wackiness { get; set; } = 10;
	[Property] public Vector3 BaseVelocity { get; set; }
	[Property] public ModelPhysics Ragdoll { get; set; }
	[Property] public bool TwirlOnStart { get; set; } = true;

	protected override void OnStart()
	{
		if ( TwirlOnStart )
		{
			DoTwirl();
		}
	}
	public void DoTwirl()
	{
		if ( !Ragdoll.IsValid() )
			return;

		for ( int i = 0; i < Wackiness; i++ )
		{
			var randomVelocity = (Vector3.Random * 100f).WithZ( 0f );
			randomVelocity *= Random.Shared.Float( 0.5f, 2f );
			var randomBodyIndex = Random.Shared.Int( 0, Ragdoll.PhysicsGroup.BodyCount - 1 );
			var randomBody = Ragdoll.PhysicsGroup.GetBody( randomBodyIndex );
			randomBody.Velocity += randomVelocity + BaseVelocity / 2;
			randomBody.AngularVelocity += Vector3.Random * 100f * Random.Shared.Float( 0.5f, 2f );
		}
	}
}
