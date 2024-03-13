using System.Linq;
using Sandbox;

public static class GameCamera
{
	public static void SetTarget( GameObject target, bool lerpToNewTarget = true )
	{
		if ( Game.ActiveScene is null )
			return;

		var shipCamera = ShipCamera.GetCurrent();
		if ( shipCamera != null )
		{
			shipCamera.Target = target;
			if ( !lerpToNewTarget )
			{
				shipCamera.ResetTransform();
			}
		}
		var fog = Game.ActiveScene.GetAllComponents<VolumetricFogVolume>().FirstOrDefault();
		if ( fog is not null )
		{
			fog.GameObject.Parent = null;
			var follower = fog.Components.GetOrCreate<Follower>();
			follower.Target = target;
		}
	}
}
