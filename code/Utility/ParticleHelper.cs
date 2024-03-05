using System.Collections.Generic;
using System.Linq;

using Sandbox;

public sealed class ParticleHelper : Component
{
	private List<ParticleEmitter> _emitters;

	public void Refresh()
	{
		_emitters = Components.GetAll<ParticleEmitter>( FindMode.EverythingInSelfAndDescendants ).ToList();
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
