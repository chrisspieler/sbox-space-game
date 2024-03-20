using Sandbox;

public sealed class DestructionEffect : Component
{
	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public Color? ParticleTint { get; set; }
	
	protected override void OnDestroy()
	{
		if ( !Game.IsPlaying )
			return;

		if ( EffectPrefab is null )
			return;

		var effectGo = EffectPrefab.Clone( Transform.World );
		effectGo.Tags.Add( "no_chunk" );
		if ( ParticleTint.HasValue )
		{
			effectGo.SetParticleTint( ParticleTint.Value );
		}
	}
}
