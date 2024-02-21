using Sandbox;

public sealed class Follower : Component
{
	[Property] public GameObject Target { get; set; }

	protected override void OnUpdate()
	{
		if ( !Target.IsValid() )
			return;

		Transform.Position = Target.Transform.Position;
	}
}
