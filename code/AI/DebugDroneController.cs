using Sandbox;

public sealed class DebugDroneController : Component
{
	protected override void OnUpdate()
	{
		if ( Input.Pressed( "grapple") )
		{
			SendDroneToMousePosition();
		}
	}

	private void SendDroneToMousePosition()
	{
		var absoluteMousePos = Scene.Camera.MouseToWorld().ToAbsolutePosition();
		foreach( var drone in Scene.GetAllComponents<Drone>() )
		{
			drone.NavTarget = Target.FromAbsolutePosition( absoluteMousePos );
		}
	}
}
