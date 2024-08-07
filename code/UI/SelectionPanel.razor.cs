﻿using Sandbox;

public partial class SelectionPanel : PanelComponent
{
	[Property] public string Image { get; set; } = "ui/img/select.svg";
	[Property] public float RotationSpeed { get; set; } = 0f;
	[Property]
	public GameObject Target
	{
		get => _target;
		set
		{
			var oldValue = _target;
			_target = value;
			if ( _target is null )
			{
				CurrentScale = 1f;
				return;
			}
			if ( oldValue != _target )
			{
				CurrentScale = _target.Transform.Scale.x;
				return;
			}
		}
	}
	private GameObject _target;

	public InputGlyphPanel GlyphPanel { get; set; }
	private float CurrentRotation { get; set; }
	private float CurrentScale { get; set; } = 1f;
	private FloatingPanel Content { get; set; }

	/// <summary>
	/// the hash determines if the system should be rebuilt. If it changes, it will be rebuilt
	/// </summary>
	protected override int BuildHash() => System.HashCode.Combine( Image, CurrentRotation, Target );

	protected override void OnUpdate()
	{
		CurrentRotation += Time.Delta * RotationSpeed;
	}

	public void AddGlyph( InputGlyphData data )
	{
		GlyphPanel?.AddGlyph( data );
	}

	public void RemoveGlyph( string actionName )
	{
		GlyphPanel?.RemoveGlyph( actionName );
	}
}
