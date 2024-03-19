using Sandbox;
using System.Linq;

public sealed class NavPlane : Component
{
	[Property] public float UpdateIntervalSeconds { get; set; } = 0.5f;
	private TimeUntil _nextNavUpdate;

	protected override void OnStart()
	{
		if ( Scene.GetAllComponents<NavPlane>().Any( p => p != this ) )
		{
			Destroy();
			return;
		}
		UpdateNavMesh();
	}

	protected override void OnUpdate()
	{
		if ( _nextNavUpdate )
		{
			UpdateNavMesh();
		}
	}

	public void UpdateNavMesh()
	{
		Scene.NavMesh.IsEnabled = true;
		Scene.NavMesh.Generate( Scene.PhysicsWorld );
		_nextNavUpdate = UpdateIntervalSeconds;
	}
}
