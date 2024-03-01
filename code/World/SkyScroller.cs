using Sandbox;

public sealed class SkyScroller : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public float ScrollFactor { get; set; } = 0.001f;

	private FloatingOriginSystem _originSystem;

	protected override void OnStart()
	{
		_originSystem = Scene.GetSystem<FloatingOriginSystem>();
	}

	protected override void OnUpdate()
	{
		Target ??= _originSystem.Origin?.GameObject;
		if ( Target is null )
			return;

		var targetPos = Target.GetAbsolutePosition();
		var forwardRotation = Rotation.FromAxis( Vector3.Right, -targetPos.x * ScrollFactor );
		var rightRotation = Rotation.FromAxis( Vector3.Forward, -targetPos.y * ScrollFactor );
		Transform.Rotation = forwardRotation * rightRotation;
	}
}
