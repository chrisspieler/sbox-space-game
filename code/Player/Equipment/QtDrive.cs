using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class QtDrive : Component
{
	[Property] public bool IsRunning => _isRunning;
	private bool _isRunning;
	[Property] public float TranslationSpeed { get; set; } = 100f;
	[Property] public Material GhostMaterial { get; set; }
	[Property] public SoundEvent LoopSound { get; set; }
	[Property] public SoundEvent BeginSound { get; set; }
	[Property] public SoundEvent EndSound { get; set; }
	[Property] public float MinUsableCharge { get; set; } = 20f;
	[Property] public float MaxCharge { get; set; } = 100f;
	[Property] public float CurrentCharge { get; set; } = 100f;
	[Property] public float ChargeGainPerSecond { get; set; } = 20f;
	[Property] public float ChargeBurnPerUseSecond { get; set; } = 50f;

	private RealTimeSince _sinceLastUpdate;
	private RealTimeSince _sinceLastClone;
	private Vector3 _newPosition;
	private GameObject _activeVisualClone;
	private Dictionary<GameObject, Color> _visualClones = new();
	private TagSet _visualCloneExcludeTags = new TagSet();
	private SoundHandle _loopSoundHandle;
	private float _targetLoopVolume;

	protected override void OnEnabled()
	{
		_loopSoundHandle = Sound.Play( LoopSound );
		_loopSoundHandle.Volume = 0f;
	}

	protected override void OnDisabled()
	{
		_loopSoundHandle.Stop( 0.3f );
	}

	protected override void OnStart()
	{
		_visualCloneExcludeTags.Add( "ragdoll" );
		_visualCloneExcludeTags.Add( "shield" );
	}

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
		_loopSoundHandle.Volume = _loopSoundHandle.Volume.LerpTo( _targetLoopVolume, 5f * _sinceLastUpdate );
		CurrentCharge = MathF.Min( MaxCharge, CurrentCharge + ChargeGainPerSecond * Time.Delta);
		_sinceLastUpdate = 0f;
	}

	private void UpdateRunning()
	{
		if ( Input.Pressed( "qt_drive" ) || CurrentCharge <= 0f )
		{
			End();
			return;
		}
		UpdateCloneColors();
		var isMoving = !Input.AnalogMove.IsNearlyZero();
		if ( isMoving )
		{
			if ( _sinceLastClone > 0.1f )
			{
				PushClone( _activeVisualClone );
			}
			CurrentCharge = MathF.Max( 0f, CurrentCharge - ChargeBurnPerUseSecond * _sinceLastUpdate );
		}
		_targetLoopVolume = isMoving ? LoopSound.Volume.GetValue() : 0f;
		var deltaPos = Input.AnalogMove * TranslationSpeed * _sinceLastUpdate;
		_newPosition += deltaPos;
		// Always draw the full color clone above the fading ones.
		_activeVisualClone.Transform.Position = _newPosition;
	}

	private void UpdateNotRunning()
	{
		if ( Input.Pressed( "qt_drive") )
		{
			if ( CurrentCharge < MinUsableCharge )
			{
				Sound.Play( EndSound, Transform.Position );
				return;
			}
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
		ScreenManager.SetHudEnabled( false );
		Scene.TimeScale = 0f;
		_isRunning = true;
		CreateActiveClone();
		PushClone( _activeVisualClone );
		_targetLoopVolume = LoopSound.Volume.GetValue();
		ScreenEffects.SetBloom( 20f );
		ScreenEffects.SetSharpness( 2f );
		Sound.Play( BeginSound, Transform.Position );
		ScreenManager.SetQtHudVisibility( true );
	}

	private void CreateActiveClone()
	{
		var ship = ShipController.GetCurrent();
		_newPosition = ship.Transform.Position;
		_activeVisualClone = ship.GameObject.VisualClone( null, _visualCloneExcludeTags, true );
		var highlight = _activeVisualClone.Components.Create<HighlightOutline>();
		highlight.Color = Color.Transparent;
		highlight.InsideObscuredColor = Color.White.WithAlpha( 0.005f );
		ship.PartsContainer.Enabled = false;
		GameCamera.SetTarget( _activeVisualClone );
	}

	private void PushClone( GameObject ship )
	{
		var clone = ship.VisualClone( null, _visualCloneExcludeTags );
		clone.Transform.Position = ship.Transform.Position;
		clone.RecursiveMaterialOverride( GhostMaterial );
		var newCloneColor = Color.Red.WithAlpha( 0.10f );
		clone.RecursiveTint( newCloneColor );
		_visualClones[clone] = newCloneColor;
		_sinceLastClone = 0f;
	}

	private void UpdateCloneColors()
	{
		// Make each previous clone's color even more red.
		foreach ( var (previousCloneGo, previousCloneColor) in _visualClones.ToList() )
		{
			var hsvColor = previousCloneColor.ToHsv();
			hsvColor.Hue += 360f - 70f * _sinceLastUpdate;
			hsvColor.Alpha -= 0.05f * _sinceLastUpdate;
			if ( hsvColor.Alpha <= 0f )
			{
				previousCloneGo.Destroy();
				_visualClones.Remove( previousCloneGo );
			}
			previousCloneGo.RecursiveTint( hsvColor );
			_visualClones[previousCloneGo] = hsvColor;
		}
	}

	public void End()
	{
		if ( !_isRunning )
			return;

		PauseMenuPanel.UnpauseTimeScale = 1f;
		ScreenManager.SetHudEnabled( true );
		Scene.TimeScale = 1f;
		_isRunning = false;
		var ship = ShipController.GetCurrent();
		ship.PartsContainer.Enabled = true;
		ship.Transform.Position = _newPosition;
		foreach( var clone in _visualClones.Keys )
		{
			clone.Destroy();
		}
		_activeVisualClone?.Destroy();
		_activeVisualClone = null;
		_targetLoopVolume = 0f;
		GameCamera.SetTarget( ship.GameObject );
		ScreenEffects.SetBloom( 1f );
		ScreenEffects.SetSharpness( 0.05f );
		Sound.Play( EndSound, Transform.Position );
		ScreenManager.SetQtHudVisibility( false );
	}
}
