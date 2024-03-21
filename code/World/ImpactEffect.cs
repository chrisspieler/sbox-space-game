using Sandbox;

public sealed class ImpactEffect : Component
{
	[Property] public GameObject Target
	{
		get => _target;
		set
		{
			var damageHasChanged = _target != value;
			_target = value;
			if ( damageHasChanged )
			{
				UpdateTarget();
			}
		}
	}
	private GameObject _target;

	private GameObject _effectInstance;

	protected override void OnUpdate()
	{ 
		if ( _effectInstance is not null )
		{
			_effectInstance.Transform.Position = Transform.Position;
			_effectInstance.Transform.Rotation = Transform.Rotation;
		}
	}

	protected override void OnDisabled()
	{
		Target = null;
	}

	private void UpdateTarget()
	{
		if ( !Game.IsPlaying )
			return;

		AbandonEffectInstance();
		if ( Target is null )
			return;

		_effectInstance = CreateEffectForSurface( Target.GetTagSurface() );
	}

	private GameObject CreateEffectForSurface( TagSurface surface )
	{
		var effectGo = surface.ContinuousImpactEffect.GetPrefabScene().Clone();
		effectGo.Tags.Add( "no_chunk" );
		effectGo.Transform.Position = Transform.Position;
		effectGo.Transform.Rotation = Transform.Rotation;
		return effectGo;
	}

	private void AbandonEffectInstance()
	{
		if ( _effectInstance is null )
			return;

		var position = _effectInstance.Transform.Position;
		_effectInstance.SetParent( null );
		_effectInstance.Transform.Position = position;
		_effectInstance.ToggleParticleEmission( false );
		foreach ( var light in _effectInstance.Components.GetAll<Light>( FindMode.EnabledInSelfAndDescendants ) )
		{
			light.Enabled = false;
		}
		var selfDestruct = _effectInstance.Components.Create<SelfDestruct>();
		selfDestruct.Delay = 5f;
		_effectInstance = null;
	}
}
