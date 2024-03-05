using System.Linq;
using System.Threading.Tasks;

namespace Sandbox;

public sealed class GizmoDrawComponent : Component
{
	public static GizmoDrawComponent Instance
	{
		get
		{
			return Game.ActiveScene
				?.GetAllComponents<GizmoDrawComponent>()
				?.FirstOrDefault();
		}
	}

	public async Task DrawFollowText( string text, GameObject followTarget, float duration )
	{
		TimeUntil untilStop = duration;
		while ( !untilStop )
		{
			if ( !followTarget.IsValid() )
				return;

			using ( Gizmo.Scope( "FollowText", followTarget.Transform.World ) )
			{
				Gizmo.Draw.Color = Color.Yellow;
				Gizmo.Draw.Text( text, global::Transform.Zero, "Consolas" );
			}
			await Task.Frame();
		}
	}
}
