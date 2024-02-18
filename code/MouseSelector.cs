using Sandbox;
using System;

public sealed class MouseSelector : Component
{
	[Property] public Action<GameObject> OnHoveredChanged { get; set; }
	
	[Property] public GameObject Hovered => _hovered;
	private GameObject _hovered;
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public TagSet FilterWithoutAny { get; set; }
	[Property] public SelectionPanel HoveredIcon { get; set; }

	protected override void OnUpdate()
	{
		var hovered = GetHovered();
		if ( hovered != _hovered )
		{
			_hovered = hovered;
			if ( HoveredIcon is not null )
			{
				HoveredIcon.Target = _hovered;
				HoveredIcon.Enabled = _hovered.IsValid();
			}
			OnHoveredChanged?.Invoke( _hovered );
		}
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
