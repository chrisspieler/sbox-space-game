using Sandbox;
using System.Linq;

public sealed class RopeTester : Component
{
	[Property] public RopePath Rope { get; set; }
	[Property] public GameObject Gun { get; set; }
	[Property] public Model HookModel { get; set; } = Model.Load( "data/equipment/grapple_rope/grapple_rope_hook.vmdl" );
	[Property] public float MoveSpeed { get; set; } = 100f;

	private GameObject _hookInstance;

	[ConVar( "rope_debug" )]
	public static bool RopeDebug { get; set; } = false;

	protected override void OnUpdate()
	{
		Rope ??= Components.GetOrCreate<RopePath>();
		Gun ??= GameObject;

		var mousePos = Scene.Camera.MouseToWorld();
		UpdateGizmos( mousePos );
		UpdateGunInput( mousePos );
		UpdateHook();
		UpdateMovement();
	}

	private void UpdateGizmos( Vector3 mousePos )
	{
		Gizmo.Draw.Color = Color.Blue;
		Gizmo.Draw.LineSphere( new Sphere( mousePos, 1.5f ) );
		Gizmo.Draw.Color = Color.Yellow;
		Gizmo.Draw.ScreenText( "LMB: Fire Rope, RMB: Clear Rope, MMB: Split Rope", new Vector2( Screen.Width / 2, Screen.Height - 25 ), "Consolas", 12, TextFlag.Center );
	}

	private void UpdateGunInput( Vector3 mousePos )
	{
		if ( Input.Pressed( "fire" ) )
		{
			Rope.Clear();
			Rope.ExtendRope( mousePos );
		}
		if ( Input.Pressed( "grapple" ) )
		{
			Rope.Clear();
		}
		if ( Input.Pressed( "qt_drive" ) )
		{
			var nearestSegment = Rope.GetNearestSegment( mousePos );
			if ( nearestSegment.IsValid() )
			{
				Rope.SplitSegment( nearestSegment, mousePos );
			}
		}
	}

	private void UpdateHook()
	{
		if ( Rope.IsEmpty )
		{
			_hookInstance?.Destroy();
		}
		else
		{
			if ( !_hookInstance.IsValid() )
			{
				_hookInstance = new GameObject( true, "Rope Hook" );
				_hookInstance.Components.Create<ModelRenderer>().Model = HookModel;
			}
			var lastSegment = Rope.Segments.LastOrDefault();
			if ( lastSegment?.NextPoint is null ) return;
			var dir = (lastSegment.NextPoint.Transform.Position - lastSegment.GameObject.Transform.Position).Normal;
			_hookInstance.Transform.Position = lastSegment.NextPoint.Transform.Position;
			_hookInstance.Transform.Rotation = Rotation.LookAt( dir );
		}
	}

	private void UpdateMovement()
	{
		Gun.Transform.Position += Input.AnalogMove * Time.Delta * MoveSpeed;
		Gun.Transform.Rotation = Rotation.LookAt( Scene.Camera.MouseToWorld() );
	}
}
