using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RandomChancer<T> : IEnumerable<RandomChancer<T>.RandomItem>
{
	public struct RandomItem
	{
		public RandomItem( T item, float ratio )
		{
			Item = item;
			Ratio = ratio;
		}

		public T Item { get; set; }
		public float Ratio { get; set; }
	}
	private List<RandomItem> _ratios = new();

	private float _ratioSum => _ratios.Sum( p => p.Ratio );

	public int Count => _ratios.Count;

	public void AddItem( RandomItem item )
	{
		_ratios.Add( item );
	}

	public void AddItem( T item, float ratio )
	{
		_ratios.Add( new RandomItem( item, ratio ) );
	}

	public T GetNext()
	{
		// Find a random "goal" within the sum of all of the item ratios.
		var numValue = Random.Shared.NextSingle() * _ratioSum;
		T selectedItem = default;
		foreach ( RandomItem item in _ratios )
		{
			// Move towards the goal we chose.
			numValue -= item.Ratio;

			// If an item pushes us past the goal...
			if ( numValue <= 0 )
			{
				//...then that's the item we choose.
				selectedItem = item.Item;
				break;
			}
			// Naturally, items with a higher ratio have a higher chance of
			// being the one that pushes us past the goal.
		}
		return selectedItem;
	}

	public IEnumerator<RandomItem> GetEnumerator()
	{
		return _ratios.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _ratios.GetEnumerator();
	}
}
