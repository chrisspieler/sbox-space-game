using Sandbox;

public sealed class PostProcessingController : Component
{
	[ConVar( "bloom_intensity" )]
	public static float BloomIntensity { get; set; } = 1f;

	[Property] public Bloom Bloom { get; set; }

	protected override void OnUpdate()
	{
		Bloom.Strength = 3f * BloomIntensity;
	}
}
