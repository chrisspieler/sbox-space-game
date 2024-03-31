using Sandbox;

public static class Metric
{
	public static float InchesToMeters( float inches ) => MathX.InchToMeter( inches );

	public static string FormatDistance( float distanceInMeters )
	{
		return distanceInMeters >= 1_000f
		? $"{(distanceInMeters / 1_000f):F2}km"
		: $"{distanceInMeters:F0}m";
	}
}
