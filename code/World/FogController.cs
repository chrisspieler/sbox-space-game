using Sandbox;

public sealed class FogController : Component
{
	[ConVar( "volumetric_fog_intensity" )]
	public static float IntensityScale { get; set; } = 1f;

	[Property] public VolumetricFogVolume Volume { get; set; }
	[Property] public float Intensity { get; set; } = 0.5f;
	protected override void OnUpdate()
	{
		Volume.Strength = (Intensity * IntensityScale).Remap( 0f, 1f, 0f, 0.2f );
	}
}
