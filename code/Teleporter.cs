using Sandbox;

public sealed class Teleporter : Component
{
	[Property] public Vector3 AbsoluteDestination { get; set; } = Vector3.Zero;

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "reload" ) )
		{
			Teleport();
		}
	}

	public void Teleport()
	{
		GameObject.SetAbsolutePosition( AbsoluteDestination );
	}
}
