using Sandbox;
using System;

public sealed class SineHover : Component
{
	[Property] public Vector3 Amplitude { get; set; }
	[Property] public Vector3 Frequency { get; set; }

	protected override void OnUpdate()
	{
		var translation = Vector3.Zero;
		translation.x = MathF.Sin( Time.Now * Frequency.x ) * Amplitude.x;
		translation.y = MathF.Sin( Time.Now * Frequency.y ) * Amplitude.y;
		translation.z = MathF.Sin( Time.Now * Frequency.z ) * Amplitude.z;
		Transform.LocalPosition = translation;
	}
}
