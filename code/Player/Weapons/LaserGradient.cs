using Sandbox;

[GameResource( "Laser Gradient", "gradient", "A gradient that offers different colors over time.", Icon = "gradient" )]
public class LaserGradient : GameResource
{
	public string Name { get; set; }
	public Gradient Gradient { get; set; }
	public float DurationSeconds { get; set; } = 5f;

	public Color GetColor()
	{
		return Gradient.Evaluate( Time.Now / DurationSeconds % 1f );
	}
}
