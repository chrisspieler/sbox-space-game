using System.Collections.Generic;
using System.Linq;

using Sandbox;

public sealed class ParticleHelper : Component
{
	private List<ParticleEmitter> _emitters;
	private List<ParticleEffect> _effects;
	private Dictionary<ParticleEffect, ParticleFloat> _originalScales = new();

	public void Refresh()
	{
		_emitters = Components.GetAll<ParticleEmitter>( FindMode.EverythingInSelfAndDescendants ).ToList();
		_effects = Components.GetAll<ParticleEffect>( FindMode.EverythingInSelfAndDescendants ).ToList();
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

	public void SetScale( float scale )
	{
		if ( _effects is null )
		{
			Refresh();
		}
		foreach ( var effect in _effects )
		{
			if ( !_originalScales.ContainsKey( effect ) )
			{
				_originalScales[effect] = effect.Scale;
			}
			// The constant comes from decompiling ParticleEffect, so don't ask me why we need it.
			var timeDelta = Time.Delta.Clamp( 0f, 0.0333333351f ) * effect.TimeScale;
			var baseScale = _originalScales[effect].Evaluate( timeDelta, 1f );
			effect.Scale = new ParticleFloat()
			{
				Type = ParticleFloat.ValueType.Constant,
				ConstantValue = scale * baseScale
			};
		}
	}
}
