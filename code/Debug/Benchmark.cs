using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class Benchmark : IEnumerable<TimeSpan>
{
	public IEnumerable<TimeSpan> Times => _times.AsReadOnly();
	private readonly List<TimeSpan> _times = new();
	public TimeSpan? ShortestTime { get; private set; }
	public TimeSpan? LongestTime { get; private set; }
	public TimeSpan? AverageTime => _times.Any()
		? TimeSpan.FromTicks( _times.Sum( t => t.Ticks ) / _times.Count )
		: null;
	public int Count => _times.Count;
	public TimeSpan? TotalTime => _times.Any()
		? TimeSpan.FromTicks( _times.Sum( t => t.Ticks ) )
		: null;

	public IDisposable Record()
	{
		return new BenchmarkScope( this );
	}

	public void Clear()
	{
		_times.Clear();
		ShortestTime = null;
		LongestTime = null;
	}

	public void PushTime( TimeSpan time )
	{
		_times.Add( time );
		if ( ShortestTime is null || time.Ticks < ShortestTime.Value.Ticks )
		{
			ShortestTime = time;
		}
		if ( LongestTime is null || time.Ticks > LongestTime.Value.Ticks )
		{
			LongestTime = time;
		}
	}

	public IEnumerator<TimeSpan> GetEnumerator()
	{
		return _times.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _times.GetEnumerator();
	}

	private class BenchmarkScope : IDisposable
	{
		public BenchmarkScope( Benchmark benchmark )
		{
			_sw = new Stopwatch();
			_sw.Start();
			_benchmark = benchmark;
		}

		private readonly Stopwatch _sw;
		private readonly Benchmark _benchmark;

		public void Dispose()
		{
			_sw.Stop();
			_benchmark.PushTime( _sw.Elapsed );
		}
	}
}
