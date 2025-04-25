using Sandbox;

public sealed class PostProcessingController : Component
{
	[ConVar( "bloom_intensity" )]
	public static float BloomIntensity { get; set; } = 1f;
	[ConVar( "sharpen_intensity" )]
	public static float SharpenIntensity { get; set; } = 1f;

	[Property] public Highlight HighlightEffect { get; set; }
	[Property] public Bloom Bloom { get; set; }
	[Property] public float TargetBloom { get; set; } = 0.5f;
	[Property] public Sharpen Sharpen { get; set; }
	[Property] public float TargetSharpness { get; set; } = 0.05f;

	public static PostProcessingController Instance { get; private set; }
	public static PostProcessingController GetCurrent() => Instance;

	protected override void OnAwake()
	{
		Instance = this;
		GameObject.BreakFromPrefab();
	}

	protected override void OnUpdate()
	{
		HighlightEffect.SetEnabled( AsteroidHighlight.GlobalStrength > 0f );
		var bloomAmount = Bloom.Strength.LerpTo( TargetBloom, RealTime.Delta * 10f );
		Bloom.Strength = bloomAmount * BloomIntensity;
		var sharpenAmount = Sharpen.Scale.LerpTo( TargetSharpness, RealTime.Delta * 5f );
		Sharpen.Scale = sharpenAmount * SharpenIntensity;
	}
}
