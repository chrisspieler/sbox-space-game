using Sandbox;

public sealed class EffectSpawner : Component, IDestructionListener
{
	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public Color? ParticleTint { get; set; }
	[Property] public bool FireOnEnabled { get; set; }
	[Property] public bool FireOnMakeDebris { get; set; }
	[Property] public RangedFloat FireDelay { get; set; } = 1f;
	[Property] public bool DestroyGameObjectAfterFire { get; set; }

	private TimeUntil? _untilShouldFire;

	protected override void OnEnabled()
	{
		if ( FireOnEnabled )
		{
			_untilShouldFire = FireDelay.GetValue();
		}
	}

	public void OnMakeDebris()
	{
		if ( FireOnMakeDebris )
		{
			_untilShouldFire = FireDelay.GetValue();
		}
	}

	protected override void OnUpdate()
	{
		if ( _untilShouldFire.HasValue && _untilShouldFire.Value )
		{
			SpawnEffect();
		}
	}

	public void SpawnEffect()
	{
		if ( EffectPrefab is null )
			return;

		var effectGo = EffectPrefab.Clone( Transform.World );
		effectGo.Tags.Add( "no_chunk" );
		if ( ParticleTint.HasValue )
		{
			effectGo.SetParticleTint( ParticleTint.Value );
		}
		if ( DestroyGameObjectAfterFire )
		{
			GameObject.Destroy();
		}
	}
}
