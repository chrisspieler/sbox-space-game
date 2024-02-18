using Sandbox;

public sealed class GrappleBeam : Component
{
	[Property] public SpringJoint Joint { get; set; }
	[Property] public MouseSelector MouseSelector { get; set; }
	[Property] public TagSet FilterWithAny { get; set; }
	[Property] public ParticleSystem BeamAsset { get; set; }
	[Property] public LegacyParticleSystem Particles { get; set; }
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

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "grapple" ) )
		{
			if ( CurrentTarget is not null )
				Disconnect();

			if ( MouseSelector.Hovered?.Tags?.Has( "solid" ) != true )
				return;

			Connect( MouseSelector.Hovered );
		}
	}

	private void Disconnect()
	{
		Joint.Body = null;
		Joint.Enabled = false;
		_currentTarget = null;
		DestroyBeam();
	}

	private void Connect( GameObject target )
	{
		if ( !target.IsValid() || !target.Tags.HasAny( FilterWithAny ) )
			return;

		_currentTarget = target;
		Joint.Body = _currentTarget;
		Joint.MinLength = 0f;
		Joint.MaxLength = Joint.Transform.Position.Distance( target.Transform.Position );
		Joint.Enabled = true;
		CreateBeam();
	}

	private void CreateBeam()
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
				FloatValue = -2f,
				StringCP = "2"
			}
		};
		Particles.Enabled = true;
	}

	private void DestroyBeam()
	{
		if ( !Particles.IsValid() )
			return;

		Particles.Enabled = false;
	}
}
