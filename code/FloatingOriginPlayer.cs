using Sandbox;

public sealed class FloatingOriginPlayer : Component
{
	[Property] public float WorldResetDistance { get; set; } = 500f;
	public FloatingOriginSystem OriginSystem => Scene.GetSystem<FloatingOriginSystem>();

	/// <summary>
	/// The position of the GameObject relative to the "true" world origin,
	/// which is the origin of the world before any floating origin shifts.
	/// </summary>
	public Vector3 AbsolutePosition
	{
		get => GameObject.GetAbsolutePosition();
		set => GameObject.SetAbsolutePosition( value );
	}
}
