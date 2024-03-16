using Sandbox;

public sealed class GrappleBeam : Component
{
	[ConVar( "grapple_debug" )]
	public static bool Debug { get; set; }

	[Property] public SpringJoint Joint { get; set; }
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public ParticleSystem BeamAsset { get; set; }
	[Property] public LegacyParticleSystem Particles { get; set; }
	[Property] public SoundEvent AttachSound { get; set; }
	[Property] public GameObject GrapplePoint { get; set; }
	[Property] public GameObject CurrentTarget => _currentTarget;

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
		var distance = Transform.Position.Distance( target.Transform.Position );
		var jointLength = distance;
		if ( distance > 500f  )
		{
			jointLength *= 0.9f;
		}
		Joint.Body = _currentTarget;
		Joint.MinLength = 0f;
		Joint.MaxLength = jointLength;
		Joint.Enabled = true;
		if ( Debug )
		{
			Log.Info( $"Grappled {target.Name}, distance {distance}, joint length {jointLength}" );
		}
		CreateParticleRope();
		CreateLight();
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
