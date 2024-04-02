using Sandbox.Razor;
using Sandbox;
using Sandbox.UI;
using System;

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

		var screenNormal = Scene.Camera.PointToScreenNormal( worldPos );
		var xInBounds = screenNormal.x > 0f && screenNormal.x < 1f;
		var yInBounds = screenNormal.y > 0f && screenNormal.y < 1f;
		if ( xInBounds && yInBounds )
			return screenNormal;

		// If the position we're trying to find is far from where the center of the screen
		// intersects the world plane, then just make up something for the normal value.
		var dir = (worldPos - screenCenterWorldPos).WithZ( 0f ).Normal;
		// Worldspace x,y -> screenspace -y,-x
		dir = new Vector2( -dir.y, -dir.x );
		// On a scale from 0 - 1, how diagonal (not cardinal) is the direction from screen center to target?
		var diagonality = MathF.Abs( MathF.Sin( dir.EulerAngles.yaw.DegreeToRadian() * 2 ) );
		// A unit vector pointed in a perfectly diagonal direction has a length of 1 / sqrt(2), or 0.70710
		// To compensate for this loss of length, we can scale the vector by sqrt(2), or 1.41421
		const float sqrt2 = 1.41421f;
		dir *= MathF.Max( 1f, sqrt2 * diagonality );
		// Remap -1, 1 to 0, 1
		dir = ( dir + 1f ) / 2f;
		return dir;
	}
}
