using System;
using Sandbox;

public sealed class SpaceFloater : Component
{
	[Property] public bool RandomizeRotation { get; set; } = true;
	[Property] public bool RandomizeScale { get; set; } = true;
	[Property] public Curve ScaleCurve { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public NavBlocker NavBlock { get; set; }

	protected override void OnStart()
	{
		Rigidbody ??= Components.GetOrCreate<Rigidbody>( FindMode.EverythingInSelf );

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
		if ( NavBlock.IsValid() )
		{
			NavBlock.Enabled = true;
		}
	}
	private void SetHealthFromScale()
	{
		var health = Components.GetOrCreate<Health>();
		health.MaxHealth *= Transform.Scale.x;
		health.CurrentHealth = health.MaxHealth;
	}
}
