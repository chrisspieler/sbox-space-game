using Sandbox;

public sealed class NavBlocker : Component
{
	[Property] public GameObject Target { get; set; }
	private GameObject _navBlockInstance;

	protected override void OnEnabled()
	{
		_navBlockInstance.SetEnabled( true );
	}

	protected override void OnDisabled()
	{
		_navBlockInstance.SetEnabled( false );
	}

	protected override void OnDestroy()
	{
		_navBlockInstance?.Destroy();
	}

	protected override void OnValidate()
	{
		Target ??= GameObject;
	}

	protected override void OnUpdate()
	{
		_navBlockInstance ??= CreateNavBlocker();
		_navBlockInstance.Transform.Rotation = Target.Transform.Rotation;
		_navBlockInstance.Transform.Scale = Target.Transform.Scale;
	}

	private GameObject CreateNavBlocker()
	{
		var bounds = Target.GetBounds();
		var blockerGo = new GameObject();
		blockerGo.Tags.Add( "nav_block" );
		blockerGo.Transform.Scale = Target.Transform.Scale;
		var follower = blockerGo.Components.Create<Follower>();
		follower.Target = Target;
		var collider = blockerGo.Components.Create<BoxCollider>();
		collider.Center = bounds.Center;
		collider.Scale = bounds.Size;
		return blockerGo;
	}
}
