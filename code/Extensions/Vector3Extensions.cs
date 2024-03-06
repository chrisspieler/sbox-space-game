using Sandbox;

public static class Vector3Extensions
{
	public static Vector3 Direction( this Vector3 from, Vector3 to )
	{
		return (to - from).Normal;
	}

	public static Vector3 ToAbsolutePosition( this Vector3 relativePosition )
	{
		if ( Game.ActiveScene is null )
			return relativePosition;

		return Game.ActiveScene
			.GetSystem<FloatingOriginSystem>()
			.RelativeToAbsolute( relativePosition );
	}

	public static Vector3 ToRelativePosition( this Vector3 absolutePosition )
	{
		if ( Game.ActiveScene is null )
			return absolutePosition;

		return Game.ActiveScene
			.GetSystem<FloatingOriginSystem>()
			.AbsoluteToRelative( absolutePosition);
	}
}
