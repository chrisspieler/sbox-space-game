using Sandbox;
using System;

public sealed class MouseSelector : Component
{
	public static MouseSelector Instance { get; private set; }

	[Property] public Action<GameObject> OnHoveredChanged { get; set; }
	
	[Property] public GameObject Hovered => _hovered;
	private GameObject _hovered;
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public TagSet FilterWithoutAny { get; set; }
	[Property] public float DeselectTime { get; set; } = 1f;

	private TimeUntil _untilDeselect;

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnUpdate()
	{
		var hovered = GetHovered();
		if ( hovered == _hovered )
		{
			_untilDeselect = DeselectTime;
			return;
		}

		// If something is hovered, don't immediately clear Hovered when there's nothing else to replace it.
		// Having this grace period makes it easier to hover over small objects in empty space.
		if ( _hovered.IsValid() && !hovered.IsValid() && !_untilDeselect )
			return;

		SetHovered( hovered );
	}

	protected override void OnDisabled()
	{
		SetHovered( null );
	}

	public void SetHovered( GameObject hovered )
	{
		_hovered = hovered;
		OnHoveredChanged?.Invoke( _hovered );
	}

	private GameObject GetHovered()
	{
		var ray = Scene.Camera.ScreenPixelToRay( Mouse.Position );
		var tr = Scene.Trace
			.Ray( ray, 3000f )
			.WithAnyTags( FilterWithAny )
			.WithoutTags( FilterWithoutAny )
			.Run();
		return tr.GameObject;
	}
}
