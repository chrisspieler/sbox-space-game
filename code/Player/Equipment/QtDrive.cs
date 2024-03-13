using Sandbox;
using System.Collections.Generic;
using System.Linq;

public sealed class QtDrive : Component
{
	[Property] public bool IsRunning => _isRunning;
	private bool _isRunning;
	[Property] public float TranslationSpeed { get; set; } = 100f;
	[Property] public Material GhostMaterial { get; set; }

	private RealTimeSince _sinceLastFrame;
	private RealTimeSince _sinceLastClone;
	private Vector3 _newPosition;
	private GameObject _activeVisualClone;
	private Dictionary<GameObject, Color> _visualClones = new();

	protected override void OnUpdate()
	{
		if ( _isRunning )
		{
			UpdateRunning();
		}
		else
		{
			UpdateNotRunning();
		}
		_sinceLastFrame = 0f;
	}

	private void UpdateRunning()
	{
		if ( Input.Pressed( "qt_drive" ) )
		{
			End();
			return;
		}
		var ship = ShipController.GetCurrent();
		if ( _sinceLastClone > 0.1f )
		{
			PushClone( ship.GameObject );
		}
		var deltaPos = Input.AnalogMove * TranslationSpeed * _sinceLastFrame;
		_newPosition += deltaPos;
		_activeVisualClone.Transform.Position += deltaPos;
	}

	private void UpdateNotRunning()
	{
		if ( Input.Pressed( "qt_drive") )
		{
			Begin();
		}
	}

	public bool CanBegin()
	{
		return Scene.TimeScale > 0f;
	}

	public void Begin()
	{
		if ( !CanBegin() )
			return;

		PauseMenuPanel.UnpauseTimeScale = 0f;
		Scene.TimeScale = 0f;
		_isRunning = true;
		var ship = ShipController.GetCurrent();
		ship.PartsContainer.Enabled = false;
		_newPosition = ship.Transform.Position;
		PushClone( ship.GameObject );
	}

	private void PushClone( GameObject ship )
	{
		// Make each previous clone's color even more red.
		foreach( var ( previousCloneGo, previousCloneColor ) in _visualClones.ToList() )
		{
			var hsvColor = previousCloneColor.ToHsv();
			hsvColor.Hue += 6.5f;
			hsvColor.Alpha -= 0.01f;
			if ( hsvColor.Alpha <= 0f )
			{
				previousCloneGo.Destroy();
				_visualClones.Remove( previousCloneGo );
			}
			previousCloneGo.RecursiveTint( hsvColor );
			_visualClones[previousCloneGo] = hsvColor;
		}
		var exclude = new TagSet();
		exclude.Add( "ragdoll" );
		exclude.Add( "equipment" );
		var clone = ship.VisualClone( null, exclude );
		clone.RecursiveMaterialOverride( GhostMaterial );
		var newCloneColor = Color.Blue.WithAlpha( 0.20f );
		clone.RecursiveTint( newCloneColor );
		_visualClones[clone] = newCloneColor;
		_activeVisualClone = clone;
		_activeVisualClone.Transform.Position = _newPosition;
		_sinceLastClone = 0f;
	}

	public void End()
	{
		if ( !_isRunning )
			return;

		PauseMenuPanel.UnpauseTimeScale = 1f;
		Scene.TimeScale = 1f;
		_isRunning = false;
		var ship = ShipController.GetCurrent();
		ship.PartsContainer.Enabled = true;
		ship.Transform.Position = _newPosition;
		foreach( var clone in _visualClones.Keys )
		{
			clone.Destroy();
		}
		_activeVisualClone = null;
	}
}
