using Sandbox;

public sealed class LightBall : Component
{
	[Property] public ModelRenderer Model { get; set; }
	[Property] public PointLight Light { get; set; }

	protected override void OnStart()
	{
		var color = new ColorHsv()
			.WithHue( Game.Random.Float( 0f, 360f ) )
			.WithSaturation( Game.Random.Float( 0.75f, 1f ) )
			.WithValue( Game.Random.Float( 0.5f, 1f ) )
			.WithAlpha( 1f )
			.ToColor();
		Model.Tint = color;
		Light.LightColor = color;
	}
}
