namespace Sandbox;

public static class GizmoDrawExtensions
{
	public static void FollowText( this Gizmo.GizmoDraw draw, string text, GameObject followTarget, float duration = 1f )
	{
		_ = GizmoDrawComponent.Instance?.DrawFollowText( text, followTarget, duration );
	}
}
