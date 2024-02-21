using Sandbox;
using System.Collections.Generic;

public sealed class MeatDresser : Component
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] public List<Clothing> Clothes { get; set; } = new();

	protected override void OnEnabled()
	{
		var container = ClothingContainer.CreateFromLocalUser();
		foreach( var clothing in Clothes )
		{
			AddToContainer( container, clothing );
		}
		container.Apply( Model );
	}

	private void AddToContainer( ClothingContainer container, Clothing clothing )
	{
		container.Clothing.RemoveAll( ( Clothing x ) => !x.CanBeWornWith( clothing ) );
		container.Clothing.Add( clothing );
	}
}
