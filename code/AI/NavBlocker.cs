using Sandbox;

public sealed class NavBlocker : Component
{

	private PhysicsBody _keyframeBody;

	protected override void OnEnabled()
	{
		_keyframeBody = new PhysicsBody( Scene.PhysicsWorld )
		{
			UseController = false,
			BodyType = PhysicsBodyType.Keyframed,
			Sleeping = false,
			AutoSleep = false,
			Transform = Transform.World
		};
		_keyframeBody.SetComponentSource( this );
		var bbox = GameObject.GetBounds();
		bbox = BBox.FromPositionAndSize( bbox.Center, bbox.Size * Transform.Scale );
		var boxShape = _keyframeBody.AddBoxShape( bbox, Transform.Rotation );
		boxShape.Tags.Add( "nav_block" );
		Transform.OnTransformChanged += UpdateTransform;
	}

	private void UpdateTransform()
	{
		_keyframeBody.Transform = Transform.World;
	}

	protected override void OnDisabled()
	{
		ClearBody();
	}

	protected override void OnDestroy()
	{
		ClearBody();
	}

	private void ClearBody()
	{
		_keyframeBody?.Remove();
		Transform.OnTransformChanged -= UpdateTransform;
	}
}
