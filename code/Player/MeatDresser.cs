using Sandbox;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Dresses a Citizen model in the specified clothing using the avatar clothing data of 
/// the local user as a base.
/// </summary>
public sealed class MeatDresser : Component
{
	/// <summary>
	/// The Citizen model that shall be dressed using the local connection avatar clothing.
	/// </summary>
	[Property] public SkinnedModelRenderer Model { get; set; }
	/// <summary>
	/// A list of clothes that shall be applied on top of the existing avatar data of the
	/// local user. Conflicting <see cref="Clothing"/> items in the avatar data will be removed.
	/// </summary>
	[Property] public List<Clothing> Clothes { get; set; } = new();

	protected override void OnEnabled()
	{
		var container = ClothingContainer.CreateFromLocalUser();
		if ( !IsBodyFullyCovered( container ) )
		{
			foreach ( var clothing in Clothes )
			{
				AddToContainer( container, clothing );
			}
		}
		container.Apply( Model );
	}
	
	/// <summary>
	/// Returns true if any of the articles of <see cref="Clothing"/> within the given
	/// <see cref="ClothingContainer"/> are full body clothing as defined by <see cref="IsFullBodyClothing(Clothing)"/>.
	/// </summary>
	/// <param name="container"></param>
	/// <returns></returns>
	public static bool IsBodyFullyCovered( ClothingContainer container )
	{
		return container.Clothing
			.Select( c => c.Clothing )
			.Any( IsFullBodyClothing );
	}

	/// <summary>
	/// Returns true if the given <see cref="Clothing"/> completely covers the body 
	/// without skin or any other customization showing through. In essence, this is
	/// the only part of the outfit that actually has any effect on the avatar's appearance.
	/// </summary>
	public static bool IsFullBodyClothing( Clothing clothing )
	{
		var resName = clothing.ResourceName;
		return resName.Contains( "mascot" )
			|| resName.Contains( "space_suit_outfit" );
	}

	private static void AddToContainer( ClothingContainer container, Clothing clothing, bool removeIncompatible = true )
	{
		if ( removeIncompatible )
		{
			container.Clothing.RemoveAll( x => !x.Clothing.CanBeWornWith( clothing ) );
		}
		container.Clothing.Add( new(clothing) );
	}
}
