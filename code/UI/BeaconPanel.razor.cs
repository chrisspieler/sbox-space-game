using Sandbox;

public partial class BeaconPanel : PanelComponent
{
	[Property] public Target Target { get; set; }
	[Property] public string Name { get; set; }
	[Property] public string BeaconId { get; set; }

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( Target );

	public string GetDistanceText()
	{
		if ( !Target.IsValid() )
			return string.Empty;

		var metersFromPlayer = Target.GetMetersFromOrigin();
		return Metric.FormatDistance( metersFromPlayer );
	}
}
