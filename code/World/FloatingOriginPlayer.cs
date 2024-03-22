using Sandbox;

public sealed class FloatingOriginPlayer : Component
{
	public FloatingOriginSystem OriginSystem => Scene.GetSystem<FloatingOriginSystem>();

	public static FloatingOriginPlayer Instance { get; private set; }

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnEnabled()
	{
		Instance = this;
	}

	protected override void OnDisabled()
	{
		if ( Instance == this )
		{
			Instance = null;
		}
	}

	protected override void OnDestroy()
	{
		if ( Instance == this )
		{
			Instance = null;
		}
	}

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
