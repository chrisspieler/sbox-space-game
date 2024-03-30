using Sandbox;

/// <summary>
/// Continuously locks this component's <see cref="GameObject"/> to the specified cardinal planes.
/// </summary>
public sealed class PlaneConstraint : Component
{
	/// <summary>
	/// If true, the X position will be set to zero.
	/// </summary>
	[Property] public bool LockX { get; set; }
	/// <summary>
	/// If true, the Y position will be set to zero.
	/// </summary>
	[Property] public bool LockY { get; set; }
	/// <summary>
	/// If true, the Z position will be set to zero.
	/// </summary>
	[Property] public bool LockZ { get; set; }
	/// <summary>
	/// If true, snapping is performed in localspace rather than worldspace. This grants a significant 
	/// performance boost, particularly when this component is deep down in a hierarchy.
	/// </summary>
	[Property] public bool UseLocalPosition { get; set; }
	/// <summary>
	/// Determines in what method the locking will occur.
	/// </summary>
	[Property] public ComponentUpdateType UpdateType { get; set; }

	protected override void OnUpdate()
	{
		if ( UpdateType != ComponentUpdateType.Update )
			return;

		Update();
	}

	protected override void OnFixedUpdate()
	{
		if ( UpdateType != ComponentUpdateType.FixedUpdate )
			return;

		Update();
	}

	protected override void OnPreRender()
	{
		if ( UpdateType != ComponentUpdateType.PreRender )
			return;

		Update();
	}

	private void Update()
	{
		var scale = new Vector3()
		{
			x = LockX ? 0 : 1,
			y = LockY ? 0 : 1,
			z = LockZ ? 0 : 1
		};
		if ( UseLocalPosition )
		{
			Transform.LocalPosition *= scale;
		}
		else
		{
			Transform.Position *= scale;
		}
	}
}
