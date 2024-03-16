using Sandbox;

public sealed class Mover : Component
{
	[Property] public Vector3 Direction { get; set; }
	[Property] public float UnitsPerSecond { get; set; }

	protected override void OnUpdate()
	{
		Transform.Position += Direction.Normal * UnitsPerSecond * Time.Delta;
	}
}
