using Sandbox;
using System.Linq;

public sealed class Follower : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public Vector3 Offset { get; set; }
	[Property] public bool FollowPlayer { get; set; }

	protected override void OnUpdate()
	{
		if ( !HasTarget() )
			return;

		Transform.Position = Target.Transform.Position + Offset;
	}

	private bool HasTarget()
	{
		if ( FollowPlayer && !Target.IsValid() )
		{
			Target = Scene.GetAllComponents<FloatingOriginPlayer>()
				.FirstOrDefault()?
				.GameObject;
		}
		return Target.IsValid();
	}
}
