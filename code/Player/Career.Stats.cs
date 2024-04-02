using System;

public partial class Career
{
	public float TotalPlayTime { get; set; }
	public float FarthestDistance { get; set; }
	public float? TenKmSpeedrunTime { get; set; }
	public int TotalCreditsEarned { get; set; }
	public int AsteroidsMined { get; set; }
	public int EnemiesDestroyed { get; set; }
	public int ShipExplosions { get; set; }

	public string GetPlayTimeString()
	{
		var time = TimeSpan.FromSeconds( TotalPlayTime );
		return time.ToString( "hh\\:mm\\:ss" );
	}
}
