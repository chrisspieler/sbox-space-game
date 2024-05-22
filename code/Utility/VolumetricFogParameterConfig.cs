namespace Sandbox;

public sealed class VolumetricFogParameterConfig : Component, CameraComponent.ISceneCameraSetup
{
	[Property] public float? Anisotropy { get; set; }
	[Property] public bool ContinuousMode { get; set; } = false;
	[Property] public float? DrawDistance { get; set; }
	[Property] public float? FadeInStart { get; set; }
	[Property] public float? FadeInEnd { get; set; }
	[Property] public bool ForceNoClipmaps { get; set; } = false;
	[Property] public float? IndirectStrength { get; set; }
	[Property] public float? Scattering { get; set; }

	/// <summary>
	/// Updates the fog parameters on every frame.
	/// </summary>
	public void SetupCamera( CameraComponent camera, SceneCamera sceneCamera )
	{
		var fogParams = sceneCamera.VolumetricFog;
		if ( Anisotropy.HasValue )
		{
			fogParams.Anisotropy = Anisotropy.Value;
		}
		fogParams.ContinuousMode = ContinuousMode;
		if ( DrawDistance.HasValue )
		{
			fogParams.DrawDistance = DrawDistance.Value;
		}
		if ( FadeInStart.HasValue )
		{
			fogParams.FadeInStart = FadeInStart.Value;
		}
		if ( FadeInEnd.HasValue )
		{
			fogParams.FadeInEnd = FadeInEnd.Value;
		}
		fogParams.ForceNoClipmaps = ForceNoClipmaps;
		if ( IndirectStrength.HasValue )
		{
			fogParams.IndirectStrength = IndirectStrength.Value;
		}
		if ( Scattering.HasValue )
		{
			fogParams.Scattering = Scattering.Value;
		}
		// There's probably no need to set BakedIndirectTexture ourselves.
	}
}
