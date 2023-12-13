using Sandbox;

public sealed class SkyScroller : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public float ScrollFactor { get; set; } = 0.001f;

	protected override void OnUpdate()
	{
		var targetPos = Target.Transform.Position;
		var forwardRotation = Rotation.FromAxis( Vector3.Right, -targetPos.x * ScrollFactor );
		var rightRotation = Rotation.FromAxis( Vector3.Forward, -targetPos.y * ScrollFactor );
		Transform.Rotation = forwardRotation * rightRotation;
	}
}
