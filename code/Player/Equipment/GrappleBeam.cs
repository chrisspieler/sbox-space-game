using Sandbox;

public sealed class GrappleBeam : Component
{
	[Property] public SpringJoint Joint { get; set; }
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public ParticleSystem BeamAsset { get; set; }
	[Property] public LegacyParticleSystem Particles { get; set; }
	[Property] public SoundEvent AttachSound { get; set; }
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

	protected override void OnUpdate()
	{
		if ( _light.IsValid() && CurrentTarget.IsValid() )
		{
			_light.Position = Joint.Transform.Position.LerpTo( _currentTarget.Transform.Position, 0.5f );
		}
		if ( Input.Pressed( "grapple" ) )
		{
			var oldTarget = CurrentTarget;
			if ( CurrentTarget is not null )
			{
				Disconnect();
			}

			var mouseSelector = MouseSelector.Instance;
			if ( mouseSelector.Hovered?.Tags?.Has( "target" ) != true )
				return;

			if ( oldTarget == mouseSelector.Hovered )
				return;

			Connect( mouseSelector.Hovered );
		}
	}

	protected override void OnDestroy()
	{
		Disconnect();
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
		Joint.Body = _currentTarget;
		Joint.MinLength = 0f;
		// Don't ask me why I scale by damping. I don't know why I do anything.
		Joint.MaxLength = Transform.Position.Distance( target.Transform.Position );
		Joint.Enabled = true;
		CreateParticleRope();
		CreateLight();
	}

	private void CreateParticleRope()
	{
		if ( BeamAsset is null )
			return;

		Particles ??= Components.GetOrCreate<LegacyParticleSystem>( FindMode.EverythingInSelf );
		Particles.Particles = BeamAsset;
		Particles.ControlPoints = new()
		{
			new ParticleControlPoint()
			{
				GameObjectValue = Joint.GameObject,
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
		Particles.Enabled = true;
	}

	private void DestroyParticleRope()
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
