using System;
using Sandbox;

public sealed class SpaceFloater : Component, Component.ExecuteInEditor
{
	[Property] public bool RandomizeRotation { get; set; } = true;
	[Property] public bool RandomizeScale { get; set; } = true;
	[Property] public float MinRandomScale { get; set; } = 0.7f;
	[Property] public float MaxRandomScale { get; set; } = 1.3f;
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public bool RandomizeVelocity { get; set; } = true;
	[Property] public float RandomVelocityScale { get; set; } = 1f;
	[Property] public Vector3 InitialVelocity { get; set; }
	[Property] public bool RandomizeAngularVelocity { get; set; } = true;
	[Property] public float RandomAngularVelocityScale { get; set; } = 0.5f;
	[Property] public Vector3 InitialAngularVelocity { get; set; }

	protected override void OnEnabled()
	{
		Transform.Position = Transform.Position.WithZ( 0f );

		if ( RandomizeRotation )
		{
			Transform.Rotation = Rotation.Random;
		}
		if ( RandomizeScale )
		{
			Transform.Scale = Transform.Scale * Game.Random.Float( MinRandomScale, MaxRandomScale );
		}
	}

	protected override void OnStart()
	{
		Rigidbody ??= Components.GetOrCreate<Rigidbody>();

		Transform.Position = Transform.Position.WithZ( 0f );

		if ( RandomizeRotation )
		{
			Transform.Rotation = Rotation.Random;
		}

		if ( !GameManager.IsPlaying )
			return;

		if ( RandomizeVelocity )
			InitialVelocity = RandomVelocityScale * Vector3.Random;
		if ( RandomizeAngularVelocity )
			InitialAngularVelocity = RandomAngularVelocityScale * Vector3.Random;

		// Stick to the 2D plane
		InitialVelocity = InitialVelocity.WithZ( 0f );

		Rigidbody.Velocity = InitialVelocity;
		Rigidbody.AngularVelocity = InitialAngularVelocity;
	}
}
