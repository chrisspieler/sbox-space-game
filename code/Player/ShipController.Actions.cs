using System.Linq;

using Sandbox;

public partial class ShipController
{
	public static ShipController GetCurrent()
	{
		return GameManager.ActiveScene
			?.GetAllComponents<ShipController>()
			?.FirstOrDefault();
	}

	[ConCmd("ship_respawn")]
	public static void Respawn()
	{
		// Make the currently active ship explode, if it exists.
		GetCurrent()?.Explode();

		if ( GameManager.ActiveScene is null )
			return;

		// The ship prefab contains a camera, so destroy the existing camera to avoid a conflict.
		DestroyAllWithComponent<CameraComponent>();
		// The deathcam will fight for control over the main camera, so get rid of it.
		DestroyAllWithComponent<DeathCamConfig>();
		SpawnShip( GetSpawnTransform() );

		void DestroyAllWithComponent<T>() where T : Component
		{
			var components = GameManager.ActiveScene.GetAllComponents<T>();
			foreach( var component in components )
			{
				component.GameObject.Destroy();
			}
		}

		Vector3 GetSpawnTransform()
		{
			var spawnPoint = GameManager.ActiveScene.GetAllComponents<SpawnPoint>().FirstOrDefault();
			return spawnPoint.IsValid()
				? spawnPoint.Transform.Position
				: Vector3.Zero;
		}

		void SpawnShip( Vector3 spawnPosition )
		{
			var prefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/ship.prefab" );
			var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
			prefabScene.Clone( spawnPosition );
		}
	}

	[ConCmd("ship_teleport")]
	public static void Teleport( Vector3 position )
	{
		GetCurrent()?.GameObject?.SetAbsolutePosition( position.WithZ( 0f ) );
	}
}
