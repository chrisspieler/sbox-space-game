using System.Collections.Generic;
using System.Linq;

using Sandbox;

public sealed class ParticleHelper : Component
{
	private List<ParticleEmitter> _emitters;
	private List<ParticleEffect> _effects;

	public void Refresh()
	{
		_emitters = Components.GetAll<ParticleEmitter>( FindMode.EverythingInSelfAndDescendants ).ToList();
		_effects = Components.GetAll<ParticleEffect>( FindMode.EverythingInSelfAndDescendants ).ToList();
	}

	public void SetTint( Color tint )
	{
		if ( _effects is null )
		{
			Refresh();
		}
		foreach( var effect in _effects )
		{
			effect.Tint = tint;
		}
	}

	public void ToggleEmit( bool shouldEmit )
	{
		if ( _emitters is null )
		{
			Refresh();
		}
		foreach( var emitter in _emitters )
		{
			emitter.Enabled = shouldEmit;
		}
	}
}
