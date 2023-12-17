using Sandbox;

public sealed class FloatingOriginPlayer : Component
{
	[Property] public float WorldResetDistance { get; set; } = 500f;
	public FloatingOriginSystem OriginSystem => Scene.GetSystem<FloatingOriginSystem>();

	public Vector3 AbsolutePosition
	{
		get => GameObject.GetAbsolutePosition();
		set => GameObject.SetAbsolutePosition( value );
	}
}
