using Sandbox;

public sealed class SkyScroller : Component, Component.ExecuteInEditor
{
	[Property] public GameObject Target { get; set; }
	[Property] public float ScrollFactor { get; set; } = 0.001f;

	protected override void OnUpdate()
	{
		if ( Target is null )
			return;

		var targetPos = Target.Transform.Position;
		var originSystem = Scene?.GetSystem<FloatingOriginSystem>();
		if ( originSystem != null )
		{
			if ( originSystem?.Origin?.GameObject == Target)
			{
				targetPos = originSystem.RelativeToAbsolute( targetPos );
			}
		}
		var forwardRotation = Rotation.FromAxis( Vector3.Right, -targetPos.x * ScrollFactor );
		var rightRotation = Rotation.FromAxis( Vector3.Forward, -targetPos.y * ScrollFactor );
		Transform.Rotation = forwardRotation * rightRotation;
	}
}
