using Sandbox;

[GameResource( "Tag Surface", "tagsurf", "A surface assumed to be had by any GameObject with a certain tag.", Icon = "add_box" )]
public class TagSurface : GameResource
{
	public int Priority { get; set; } = 0;
	public PrefabFile ContinuousImpactEffect { get; set; }
	// TODO: Add effects for bigger impacts.
}
