using Sandbox;

public sealed class Weapon : Component
{
	[Property] public GameObject Body { get; set; }

	protected override void OnUpdate()
	{
		UpdateBodyRotation();
	}

	private void UpdateBodyRotation()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		var direction = (mousePos - Body.Transform.Position).Normal;
		var yaw = Rotation.LookAt( direction ).Yaw();
		Body.Transform.Rotation = ((Angles)Body.Transform.Rotation).WithYaw( yaw );
	}
}
