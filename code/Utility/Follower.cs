using Sandbox;

public sealed class Follower : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public bool FollowPlayer { get; set; }

	protected override void OnUpdate()
	{
		if ( !HasTarget() )
			return;

		Transform.Position = Target.Transform.Position;
	}

	private bool HasTarget()
	{
		if ( FollowPlayer && !Target.IsValid() )
		{
			Target = ShipController.GetCurrent()?.GameObject;
		}
		return Target.IsValid();
	}
}
