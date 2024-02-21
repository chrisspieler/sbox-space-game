namespace Sandbox;

public sealed class RigidbodyDebug : Component
{
	[Property] public Rigidbody Rigidbody { get; set; }

	protected override void OnUpdate()
	{
		using ( Gizmo.Scope( "Rigidbody Debug" ) )
		{
			Gizmo.Draw.Color = Color.Yellow;
			var text = $"Velocity: {Rigidbody.Velocity} Angular Velocity: {Rigidbody.AngularVelocity}";
			Gizmo.Draw.Text( text, Transform.World, "Consolas" );
		}
	}
}
