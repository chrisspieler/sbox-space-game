using Sandbox.UI;
using System;
using System.Collections.Generic;

public partial class CargoPriceSummary : Panel
{
	public Shop Shop { get; set; }

	public override int GetHashCode()
	{
		int hash = 0;
		foreach( var value in Career.Active.ShopCargoValues )
		{
			hash = HashCode.Combine( hash, value.Value );
		}
		return hash;
	}

	private IEnumerable<CargoValue> GetAllCargoValues()
	{
		return Shop.GetAllCargoValues();
	}
}
