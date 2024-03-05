using Sandbox;

public sealed class MoveOnSpawn : Component
{
	[Property] public Vector3 AbsolutePosition { get; set; }

	protected override void OnUpdate()
	{
		GameObject.SetAbsolutePosition( AbsolutePosition );
		Destroy();
	}
}
