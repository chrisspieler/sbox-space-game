using Sandbox;

public static class ColliderExtensions
{
	public static bool IsPlayer( this Collider collider )
	{
		if ( !collider.Tags.Has( "player" ) )
			return false;

		return collider.Components.TryGet<ShipController>( out _, FindMode.EverythingInSelfAndAncestors );
	}
}
