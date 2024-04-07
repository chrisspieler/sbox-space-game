using Sandbox;
using Sandbox.Utility;
using System;
using static Sandbox.PhysicsContact;

public sealed class GrappleBeam : Component
{
	[ConVar( "grapple_debug" )]
	public static bool Debug { get; set; }
	[ConVar( "grapple_pull_strength" )]
	public static float PullStrength { get; set; } = 0.5f;
	[ConVar( "grapple_pull_min_distance" )]
	public static float PullMinDistance { get; set; } = 500f;
	[ConVar( "grapple_pull_cross_velocity_min" )]
	public static float CrossVelocityMin { get; set; } = 0f;
	[ConVar( "gapple_pull_cross_velocity_max" )]
	public static float CrossVelocityMax { get; set; } = 1000f;
	

	[Property] public SpringJoint Joint { get; set; }
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public ParticleSystem BeamAsset { get; set; }
	[Property] public LegacyParticleSystem Particles { get; set; }
	[Property] public SoundEvent AttachSound { get; set; }
	[Property] public GameObject GrapplePoint { get; set; }
	[Property] public GameObject CurrentTarget => _currentTarget;
	[Property] public float RetractSpeed { get; set; } = 250f;

	public bool IsSlack
	{
		get
		{
			if ( !_currentTarget.IsValid() )
				return true;

			var distance = Joint.Transform.Position.Distance( _currentTarget.Transform.Position );
			return distance < Joint.MaxLength;
		}
	}
	private GameObject _currentTarget;

	private SceneLight _light;
	private RealTimeSince _realDeltaTime;

	protected override void OnUpdate()
	{
		if ( !CurrentTarget.IsValid() )
			Disconnect();

		if ( Scene.TimeScale == 0f )
		{
			if ( Particles?.Enabled == true )
			{
				Particles.SceneObject.Simulate( _realDeltaTime );
			}
			_realDeltaTime = 0f;
			return;
		}
		

		if ( _light.IsValid() && CurrentTarget.IsValid() )
		{
			_light.Position = Joint.Transform.Position.LerpTo( _currentTarget.Transform.Position, 0.5f );
		}
		var mouseSelector = MouseSelector.Instance;
		var isHoveringValidTarget = mouseSelector.Hovered?.Tags?.Has( "target" ) == true;
		var oldTarget = CurrentTarget;
		var isHoveringOldTarget = oldTarget == mouseSelector.Hovered;
		UpdateSelectionUI( isHoveringValidTarget, isHoveringOldTarget );

		if ( Input.Pressed( "grapple" ) )
		{
			if ( CurrentTarget is not null )
			{
				Disconnect();
			}

			if ( !isHoveringValidTarget )
				return;

			// Don't instantly reconnect to the target we disconnected from.
			if ( !isHoveringOldTarget )
			{
				Connect( mouseSelector.Hovered );
			}
		}

		if ( Input.Down( "grapple" ) )
		{
			Retract();
		}
	}

	protected override void OnDestroy()
	{
		Disconnect();
	}

	private void UpdateSelectionUI( bool isHoveringValid, bool isHoveringOld )
	{
		if ( CurrentTarget is null && !isHoveringValid )
		{
			ScreenManager.RemoveSelectionGlyph( "grapple" );
			return;
		}

		var glyphData = new InputGlyphData()
		{
			ActionName = "grapple",
			DisplayText = isHoveringOld
				? "Disconnect Grapple Beam"
				: "Grapple Beam",
			RemovalPredicate = () => !Enabled || !IsValid
		};
		ScreenManager.AddSelectionGlyph( glyphData );
	}

	private void Disconnect()
	{
		Joint.Body = null;
		Joint.Enabled = false;
		_currentTarget = null;
		DestroyParticleRope();
		DestroyLight();
	}

	private void Connect( GameObject target )
	{
		if ( !target.IsValid() || !target.Tags.HasAny( FilterWithAny ) )
			return;

		if ( AttachSound is not null )
		{
			Sound.Play( AttachSound, Transform.Position );
		}

		_currentTarget = target;
		
		var jointLength = GetJointLength( target );
		Joint.Body = _currentTarget;
		Joint.MinLength = 0f;
		Joint.MaxLength = jointLength;
		Joint.Enabled = true;
		if ( Debug )
		{
			Log.Info( $"Grappled {target.Name}, distance {Transform.Position.Distance( target.Transform.Position)}, joint length {jointLength}" );
		}
		CreateParticleRope();
		CreateLight();
	}

	private void Retract()
	{
		if ( !CurrentTarget.IsValid() )
			return;

		var distance = Transform.Position.Distance( CurrentTarget.Transform.Position );
		var retractFactor = distance.LerpInverse( 200f, 1000f );
		Joint.MaxLength -= RetractSpeed * retractFactor * Time.Delta;
		Joint.MaxLength = MathF.Max( 0, Joint.MaxLength );
	}

	private float GetJointLength( GameObject target )
	{
		var distance = Transform.Position.Distance( target.Transform.Position );

		if ( distance < PullMinDistance )
			return distance;

		if ( !Components.TryGet<Rigidbody>( out var rb, FindMode.InAncestors | FindMode.EnabledInSelf ) )
			return distance;

		var targetDirection = Transform.Position.Direction( target.Transform.Position );
		var dot = rb.Velocity.Normal.Dot( targetDirection );
		var alignment = MathF.Abs( dot );
		// Pull more strongly if our velocity would not lead us to or away from the target.
		var pullAmount = 0.2f * (1f - alignment);
		// Pull more strongly when our velocity is higher.
		var velocityPullScale = rb.Velocity.Length.LerpInverse( CrossVelocityMin, CrossVelocityMax );
		return distance - distance * pullAmount * velocityPullScale * PullStrength;
	}

	public void CreateParticleRope()
	{
		if ( BeamAsset is null )
			return;

		Particles ??= Components.GetOrCreate<LegacyParticleSystem>( FindMode.EverythingInSelf );
		Particles.Particles = BeamAsset;
		UpdateControlPoints();
		Particles.Enabled = true;
	}

	public void UpdateControlPoints()
	{
		Particles.ControlPoints = new()
		{
			new ParticleControlPoint()
			{
				GameObjectValue = GrapplePoint,
				StringCP = "0"
			},
			new ParticleControlPoint()
			{
				GameObjectValue = _currentTarget,
				StringCP = "1"
			},
			new ParticleControlPoint()
			{
				Value = ParticleControlPoint.ControlPointValueInput.Float,
				FloatValue = -1f,
				StringCP = "2"
			}
		};
	}

	public void DestroyParticleRope()
	{
		if ( !Particles.IsValid() )
			return;

		Particles.Enabled = false;
	}

	private void CreateLight()
	{
		if ( _light.IsValid() )
		{
			_light.Delete();
		}

		_light = new SceneLight( Scene.SceneWorld );
		_light.ShadowsEnabled = false;
		_light.LightColor = Color.Cyan;
		_light.Radius = 2000f;
	}

	private void DestroyLight()
	{
		if ( _light.IsValid() )
		{
			_light.Delete();
		}
	}
}
