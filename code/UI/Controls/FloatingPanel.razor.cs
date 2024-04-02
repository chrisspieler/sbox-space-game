using Sandbox.Razor;
using Sandbox;
using Sandbox.UI;

public partial class FloatingPanel : Panel
{
	public RenderFragment Body { get; set; }
	public Target Target { get; set; }
	public Vector2 Offset { get; set; }
	public bool ConfineToScreen { get; set; }
	public Vector2 ConfinementMargin { get; set; } = new Vector2( 0.05f );
	public bool TickSelf { get; set; } = true;

	public override void Tick()
	{
		if ( TickSelf )
		{
			OnUpdate();
		}
	}

	public void OnUpdate()
	{
		// If our target is a GameObject that's no longer valid, destroy this panel.
		if ( !Target.IsValid() )
		{
			Delete( true );
			return;
		}
		var screenNormal = GetScreenNormal( Target );
		if ( ConfineToScreen )
		{
			screenNormal = screenNormal.Clamp( ConfinementMargin, 1f - ConfinementMargin );
		}
		screenNormal += new Vector3( Offset );
		Style.Top = Length.Percent( screenNormal.y * 100 );
		Style.Left = Length.Percent( screenNormal.x * 100 );
		StateHasChanged();
	}

	private Vector3 GetScreenNormal( Target target )
	{
		if ( target.GameObject is null )
		{
			return GetScreenNormal( target.RelativePosition );
		}
		var bounds = target.GameObject.GetBounds();
		var targetPos = bounds.Size == 0f
			? target.RelativePosition
			: bounds.Center;
		return GetScreenNormal( targetPos );
	}

	private Vector2 GetScreenNormal( Vector3 worldPos )
	{
		var screenCenterWorldPos = Scene.Camera.ScreenNormalToWorld( 0.5f );

		if ( worldPos.Distance( screenCenterWorldPos ) < 2000f )
		{
			return Scene.Camera.PointToScreenNormal( worldPos );
		}
		// If the position we're trying to find is far from where the center of the screen
		// intersects the world plane, then just make up something for the normal value.
		var dir = (worldPos - screenCenterWorldPos).WithZ( 0f ).Normal;
		// Worldspace x,y -> screenspace -y,-x
		dir = new Vector2( -dir.y, -dir.x );
		// Remap -1, 1 to 0, 1
		dir += 1f;
		dir /= 2f;
		// TODO: Make the panel hug the corner of the screen instead of making a circle.
		return dir;
	}
}
