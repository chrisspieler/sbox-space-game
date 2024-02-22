using System.Linq;

using Sandbox;

public partial class ShipController
{
	public static ShipController GetCurrent()
	{
		return GameManager.ActiveScene
			.GetAllComponents<ShipController>()
			.FirstOrDefault();
	}

	[ConCmd("ship_respawn")]
	public static void Respawn()
	{
		// Make the currently active ship explode, if it exists.
		GetCurrent()?.Explode();


	}

	[ConCmd("ship_teleport")]
	public static void Teleport( Vector3 position )
	{
		GetCurrent()?.GameObject?.SetAbsolutePosition( position.WithZ( 0f ) );
	}
}
