namespace Sandbox;

public sealed class LaserBeam : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public Color Tint { get; set; }
	[Property] public ParticleSystem ParticleAsset { get; set; }
	[Property] public LegacyParticleSystem ParticleInstance { get; set; }

	protected override void OnStart()
	{
		CreateParticles();
	}

	private void CreateParticles()
	{
		ParticleInstance ??= Components.GetOrCreate<LegacyParticleSystem>( FindMode.EverythingInSelf );
		ParticleInstance.Particles = ParticleAsset;
		ParticleInstance.ControlPoints = new()
		{
			new ParticleControlPoint()
			{
				GameObjectValue = GameObject,
				StringCP = "0"
			},
			new ParticleControlPoint()
			{
				GameObjectValue = Target,
				StringCP = "1"
			},
			new ParticleControlPoint()
			{
				Value = ParticleControlPoint.ControlPointValueInput.Color,
				ColorValue = Tint,
				StringCP = "2"
			}
		};
		ParticleInstance.Enabled = true;
	}
}
