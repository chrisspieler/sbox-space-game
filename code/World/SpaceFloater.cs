﻿using System;
using Sandbox;

public sealed class SpaceFloater : Component
{
	[Property] public bool RandomizeRotation { get; set; } = true;
	[Property] public bool RandomizeScale { get; set; } = true;
	[Property] public Curve ScaleCurve { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public bool RandomizeVelocity { get; set; } = true;
	[Property] public float RandomVelocityScale { get; set; } = 1f;
	[Property] public Vector3 InitialVelocity { get; set; }
	[Property] public float MassOverride { get; set; } = 1000f;
	[Property] public bool RandomizeAngularVelocity { get; set; } = true;
	[Property] public float RandomAngularVelocityScale { get; set; } = 0.5f;
	[Property] public Vector3 InitialAngularVelocity { get; set; }
	[Property] public bool LockToPlane { get; set; } = true;

	protected override void OnStart()
	{
		Rigidbody ??= Components.GetOrCreate<Rigidbody>();

		Transform.Position = Transform.Position.WithZ( 0f );

		if ( RandomizeRotation )
		{
			Transform.Rotation = Rotation.Random;
		}
		if ( RandomizeScale )
		{
			Transform.Scale = Transform.Scale * ScaleCurve.Evaluate( Random.Shared.NextSingle() );
		}

		SetHealthFromScale();

		Rigidbody.PhysicsBody.Mass = MassOverride * Transform.Scale.Length;

		SetVelocity();
	}

	protected override void OnFixedUpdate()
	{
		if ( LockToPlane && Transform.Position.z != 0f )
		{
			Transform.Position = Transform.Position.WithZ( 0f );
		}
	}

	private void SetVelocity()
	{
		if ( RandomizeVelocity )
			InitialVelocity = RandomVelocityScale * Vector3.Random;
		if ( RandomizeAngularVelocity )
			InitialAngularVelocity = RandomAngularVelocityScale * Vector3.Random;

		// Stick to the 2D plane
		InitialVelocity = InitialVelocity.WithZ( 0f );

		Rigidbody.Velocity = InitialVelocity;
		Rigidbody.AngularVelocity = InitialAngularVelocity;
	}

	private void SetHealthFromScale()
	{
		var health = Components.GetOrCreate<Health>();
		health.MaxHealth *= Transform.Scale.x;
		health.CurrentHealth = health.MaxHealth;
	}
}
