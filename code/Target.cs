using Sandbox;

public struct Target
{
	public Target( GameObject go )
	{
		GameObject = go;
	}

	public GameObject GameObject { get; set; }
	public Vector3 AbsolutePosition
	{
		get
		{
			return GameObject?.GetAbsolutePosition()
				?? _absolutePosition;
		}
		set
		{
			_absolutePosition = value;
			GameObject = null;
		}
	}
	private Vector3 _absolutePosition;

	public Vector3 RelativePosition
	{
		get
		{
			return GameObject?.GetAbsolutePosition()
				?? AbsolutePosition.ToRelativePosition();
		}
		set
		{
			AbsolutePosition = value.ToAbsolutePosition();
			GameObject = null;
		}
	}

	public static Target FromAbsolutePosition( Vector3 absolutePos )
	{
		return new Target() { AbsolutePosition = absolutePos };
	}

	public static Target FromRelativePosition( Vector3 relativePos )
	{
		return new Target() { RelativePosition = relativePos };
	}
}
