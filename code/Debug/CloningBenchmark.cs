using Sandbox;

public sealed class CloningBenchmark : Component
{
	[Property] public GameObject Prefab { get; set; }
	[Property] public int XCount { get; set; } = 100;
	[Property] public int YCount { get; set; } = 100;
	[Property] public float Spacing { get; set; } = 200f;

	private Benchmark _benchmark;

	protected override void OnStart()
	{
		_benchmark = new Benchmark();
		RunBenchmark();
	}
	
	public void RunBenchmark()
	{
		_benchmark = new Benchmark();
		for (int x = 0; x < XCount; x++)
		{
			for ( int y = 0; y < YCount; y++ )
			{
				using var _ = _benchmark.Record();
				var position = GetPositionForCoordinates( x, y );
				var go = Prefab.Clone( position );
				go.BreakFromPrefab();
			}
		}
		PrintBenchmark();
	}

	private Vector3 GetPositionForCoordinates( int x, int y )
	{
		var xPos = x * Spacing;
		xPos -= XCount * Spacing / 2;
		var yPos = y * Spacing;
		yPos = y * Spacing / 2;
		return new Vector3( xPos, yPos, 0 );
	}

	private void PrintBenchmark()
	{
		Log.Info( $"Ran benchmark with {_benchmark.Count} times over {_benchmark.TotalTime.Value.Ticks / 10_000f}ms:" );
		Log.Info( $"Average: {_benchmark.AverageTime.Value.Ticks / 10_000f}ms, Shortest: {_benchmark.ShortestTime.Value.Ticks / 10_000f}ms, Longest: {_benchmark.LongestTime.Value.Ticks / 10_000f}ms" );
	}

}
