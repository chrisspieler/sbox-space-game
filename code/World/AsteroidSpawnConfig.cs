using Sandbox;
using System.Collections.Generic;

[GameResource( "Asteroid Spawn Config", "aspwncfg", "Determines the probability and density of asteroid spawns within a chunk.", Icon = "casino" )]
public class AsteroidSpawnConfig : GameResource
{
	public struct AsteroidType
	{
		public GameObject Prefab { get; set; }
		public float Probability { get; set; }
	}

	[Property] public List<AsteroidType> AsteroidTypes { get; set; }
	[Property, Range( 64, 512, 32 )] 
	public int Spacing { get; set; } = 160;
	[Property, Range( 0.001f, 0.1f, 0.001f )] 
	public float NoiseScale { get; set; } = 0.01f;
	[Property, Range( 0, 0.2f, 0.01f )] 
	public float ProbabilityScale { get; set; } = 0.1f;
	[Property, Range( -0.05f, 0.05f, 0.01f )] 
	public float ProbabilityBias { get; set; } = 0.01f;
}
