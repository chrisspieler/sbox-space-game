using System.Collections.Generic;

using Sandbox;

[GameResource( "Asteroid Data", "astrd", "The properties of a type of asteroid.", Icon = "blur_circular" )]
public class AsteroidData : GameResource
{
	public string Name { get; set; } = "Unknown Asteroid Type";
	public List<PrefabFile> ModelPrefabs { get; set; }
	public LootTable LootTable { get; set; }
	public Curve ScaleCurve { get; set; }
	public float MassPerScale { get; set; } = 40f;
	public float LootScale { get; set; } = 1f;
}
