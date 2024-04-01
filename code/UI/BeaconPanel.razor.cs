using Sandbox;

public partial class BeaconPanel : PanelComponent
{
	[Property] public Beacon Target { get; set; }

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( Target, Target?.Name );

	public string GetDistanceText()
	{
		if ( Target is null )
			return string.Empty;

		var metersFromPlayer = Target.GetMetersFromOrigin();
		return Metric.FormatDistance( metersFromPlayer );
	}
}
