using Sandbox;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Sandbox.Diagnostics;
using Sandbox.Utility;

public sealed class NavPlane : Component
{
	[Property] public float UpdateIntervalSeconds { get; set; } = 0.5f;
	[Property] public float Diameter { get; set; } = 10000f;
	[Property, JsonIgnore] public double AverageNavGenerationTime => _generateTilesTimes.Average();
	[Property, JsonIgnore] public double MinNavGenerationTime => _generateTilesTimes.Min();
	[Property, JsonIgnore] public double MaxNavGenerationTime => _generateTilesTimes.Max();
	[Property, JsonIgnore] public double LastNavGenerationTime => _generateTilesTimes.Back();

	private readonly CircularBuffer<double> _generateTilesTimes = new(20);
	private Task _generateTilesTask;
	private FastTimer _generateTilesTimer;

	[Property, JsonIgnore]
	public BBox Bounds => BBox.FromPositionAndSize(
		center: WorldPosition,
		size: new Vector3( Diameter, Diameter, 0.01f ) // The navmesh should be bounded to a flat plane.  
	);

	private TimeUntil _nextNavUpdate;

	protected override void OnStart()
	{
		// Don't let this component exist if an older instance of the same component exists in this scene.
		if ( Scene.GetAllComponents<NavPlane>().Any( p => p != this ) )
		{
			Destroy();
			return;
		}

		UpdateNavMesh();
	}

	protected override void OnUpdate()
	{
		if ( _generateTilesTask?.IsCompleted == true )
		{
			_generateTilesTimes.PushBack( _generateTilesTimer.ElapsedMilliSeconds );
			_generateTilesTask = null;
		}

		if ( _nextNavUpdate )
		{
			UpdateNavMesh();
		}
	}

	public void UpdateNavMesh()
	{
		// Don't generate a new nav mesh if we're still waiting on the result of the old generation.
		if ( Scene.NavMesh.IsGenerating )
			return;

		Scene.NavMesh.IsEnabled = true;
		_generateTilesTimer = FastTimer.StartNew();
		_generateTilesTask = Scene.NavMesh.GenerateTiles( Scene.PhysicsWorld, Bounds );
		_nextNavUpdate = UpdateIntervalSeconds;
	}
}
