public static class Vector3Extensions
{
	public static Vector3 Direction( this Vector3 from, Vector3 to )
	{
		return (to - from).Normal;
	}
}
