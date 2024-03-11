using Sandbox;

public sealed class Stabilizer : Component
{
	[ConVar("stabilizer_debug")]
	public static bool Debug { get; set; }
	[Property] public Rigidbody Rigidbody { get; set; }
	[Property] public Rigidbody VelocityMatchTarget { get; set; }
	[Property] public float VelocityMatchedThreshold { get; set; } = 3f;
	[Property] public float VelocityMatchMaxDistance { get; set; } = 2000f;
	[Property] public float StabilizerPower { get; set; }
	[Property] public SoundEvent LoopSound { get; set; }

	private Rigidbody _hoveredRigidbody;
	private SoundHandle _loopSoundHandle;

	protected override void OnStart()
	{
		MouseSelector.Instance.OnHoveredChanged += OnHoveredChanged;
	}

	private void OnHoveredChanged( GameObject hovered )
	{
		var rigidbody = hovered?.Components?.Get<Rigidbody>();
		if ( rigidbody == null )
		{
			// We're hovering over empty space, or something that has no velocity of its own.
			_hoveredRigidbody = null;
			return;
		}
		// If the ship is moving at a velocity different from the hovered object...
		if ( Rigidbody.Velocity.Distance( rigidbody.Velocity ) > VelocityMatchedThreshold )
		{
			// ...recognize the object as something we could potentially match velocity with.
			_hoveredRigidbody = rigidbody;
		}
	}

	protected override void OnUpdate()
	{
		UpdateSelectionUI();

		// It's not reasonable to try to match velocity with something that's off-screen.
		// How would you even know it's there?
		if ( !CanMatchTarget( VelocityMatchTarget ) )
		{
			VelocityMatchTarget = null;
		}

		UpdateSound();
		UpdateMatchText();

		if ( Input.Pressed( "stabilizer" ) )
		{
			VelocityMatchTarget = _hoveredRigidbody;
		}
	}

	protected override void OnDisabled()
	{
		_loopSoundHandle?.Stop( 0.1f );
	}

	private bool CanMatchTarget( Rigidbody rb )
	{
		return rb.IsValid()
			&& rb.Transform.Position
				.Distance( Transform.Position ) < VelocityMatchMaxDistance;
	}

	private void UpdateSelectionUI()
	{
		var selection = CanMatchTarget( _hoveredRigidbody )
			? _hoveredRigidbody.GameObject
			: VelocityMatchTarget?.GameObject;
		ScreenManager.SetHoveredSelection( selection );
	}

	private void UpdateSound()
	{
		if ( !VelocityMatchTarget.IsValid() )
		{
			_loopSoundHandle?.Stop( 0.1f );
			return;
		}
		if ( _loopSoundHandle?.IsPlaying != true )
		{
			_loopSoundHandle = Sound.Play( LoopSound, Transform.Position );
		}
		_loopSoundHandle.Position = Transform.Position;
	}

	private void UpdateMatchText()
	{
		var canMatchHovered = CanMatchTarget( _hoveredRigidbody );
		// There's no reason to draw any text if we can't cancel or start matching velocity.
		if ( !VelocityMatchTarget.IsValid() && !canMatchHovered )
			return;

		// TODO: Replace this with proper glyphs in the UI.
		using ( Gizmo.Scope( "Glyph Placeholder" ) )
		{
			Gizmo.Draw.Color = Color.Yellow;

			var position = _hoveredRigidbody?.Transform?.Position
				?? VelocityMatchTarget?.Transform?.Position
				?? Vector3.Zero;

			var message = (CanMatchTarget( _hoveredRigidbody ) && _hoveredRigidbody != VelocityMatchTarget)
				? "Press R to Match Velocity"
				: "Press R to Stop Matching Velocity";

			Gizmo.Draw.ScreenText( message, Scene.Camera.PointToScreenPixels( position ), "Consolas" );
		}
	}

	public Vector3 GetStabilizerForce()
	{
		if ( !Enabled || !VelocityMatchTarget.IsValid() || !Rigidbody.IsValid() )
			return default;

		if ( Rigidbody.Velocity.Distance(VelocityMatchTarget.Velocity) < VelocityMatchedThreshold )
		{
			VelocityMatchTarget = null;
			return Vector3.Zero;
		}

		DrawDebug( Rigidbody.Velocity );
		// Move our velocity towards the target's velocity.
		var delta = VelocityMatchTarget.Velocity - Rigidbody.Velocity;
		delta = delta.Clamp( -StabilizerPower, StabilizerPower );
		return delta;
	}

	private void DrawDebug( Vector3 currentVelocity )
	{
		if ( !Debug )
			return;

		if ( !VelocityMatchTarget.IsValid() )
			return;

		using ( Gizmo.Scope("Stabilizer Debug") )
		{
			var midpoint = Transform.Position.LerpTo( VelocityMatchTarget.Transform.Position, 0.5f );
			Gizmo.Draw.Color = Color.Red;
			Gizmo.Draw.Arrow( midpoint, midpoint + VelocityMatchTarget.Velocity );
			Gizmo.Draw.Color = Color.Blue;
			Gizmo.Draw.Arrow( midpoint, midpoint + currentVelocity );
		}

	}
}
