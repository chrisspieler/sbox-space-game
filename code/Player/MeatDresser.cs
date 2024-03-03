using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class MeatDresser : Component
{
	[Property] public SkinnedModelRenderer Model { get; set; }
	[Property] public List<Clothing> Clothes { get; set; } = new();

	protected override void OnEnabled()
	{
		var container = ClothingContainer.CreateFromLocalUser();
		if ( !container.Clothing.Any( c => c.ResourceName.Contains("mascot") ) )
		{
			foreach ( var clothing in Clothes )
			{
				AddToContainer( container, clothing );
			}
		}
		container.Apply( Model );
	}

	private void AddToContainer( ClothingContainer container, Clothing clothing )
	{
		container.Clothing.RemoveAll( ( Clothing x ) => !x.CanBeWornWith( clothing ) );
		container.Clothing.Add( clothing );
	}
}
