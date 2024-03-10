namespace Sandbox;

public class TraceResult
{
	public GameObject GameObject { get; set; }
	public bool Hit { get; set; }
	public Vector3 StartPosition { get; set; }
	public Vector3 EndPosition { get; set; }
	public Vector3 Distance { get; set; }
	public Vector3 Normal { get; set; }


	public static implicit operator TraceResult( SceneTraceResult tr )
	{
		return new TraceResult()
		{
			GameObject = tr.GameObject,
			StartPosition = tr.StartPosition,
			EndPosition = tr.EndPosition,
			Distance = tr.Distance,
			Hit = tr.Hit,
			Normal = tr.Normal
		};
	}
}
