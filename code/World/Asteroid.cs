using Sandbox;
using Sandbox.Diagnostics;
using System.Linq;

public sealed class Asteroid : Component
{
	public AsteroidData Data { get; set; }
	public float? ScaleOverride { get; set; }

	protected override void OnStart()
	{
		Assert.NotNull( Data );

		CreateModel();
		SetTransform();
		SetHealthFromScale();
		ScaledMassOverride.Apply( GameObject, Data.MassPerScale );
	}

	private void SetTransform()
	{
		Transform.Rotation = Rotation.Random;
		Transform.Scale = ScaleOverride ?? Data.ScaleCurve.Evaluate( Game.Random.Float() );
	}

	private void SetHealthFromScale()
	{
		var health = Components.Get<Health>();
		health.MaxHealth *= Transform.Scale.x;
		health.CurrentHealth = health.MaxHealth;
	}

	private void CreateModel()
	{
		var modelPrefab = Game.Random.FromList( Data.ModelPrefabs );
		var modelGo = modelPrefab.GetPrefabScene().Clone();
		modelGo.Parent = GameObject;
	}

	public static GameObject Create( AsteroidData data, float? scaleOverride = null )
	{
		var asteroidPrefab = ResourceLibrary.Get<PrefabFile>( "prefabs/asteroid_base.prefab" );
		var go = asteroidPrefab.GetPrefabScene().Clone( new Transform(), null, true, data.Name );
		var asteroidComponent = go.Components.Get<Asteroid>();
		asteroidComponent.Data = data;
		asteroidComponent.ScaleOverride = scaleOverride;
		return go;
	}

	[ConCmd("asteroid_spawn")]
	public static void SpawnCommand()
	{
		if ( Game.ActiveScene is null || Game.ActiveScene.Camera is null )
			return;

		var asteroidTypes = ResourceLibrary.GetAll<AsteroidData>().ToList();
		if ( !asteroidTypes.Any() )
			return;

		var worldPos = Game.ActiveScene.Camera.ScreenNormalToWorld( 0.5f );
		var bounds = BBox.FromPositionAndSize( worldPos, new Vector2( 2500f ) );
		for ( int i = 0; i < 20; i++ )
		{
			var asteroid = Create( Game.Random.FromList( asteroidTypes ) );
			asteroid.Transform.Position = bounds.RandomPointInside;
		}
	}
}
