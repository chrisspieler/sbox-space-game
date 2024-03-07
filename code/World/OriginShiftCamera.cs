using Sandbox;
using System;

public sealed class OriginShiftCamera : Component, IOriginShiftListener
{
	[Property] public CameraComponent Camera { get; set; }
	[Property] public int OriginShiftFreezeFrames { get; set; } = 3;
	[Property] public TagSet RenderTags { get; set; }
	[Property] public TagSet ExcludeTags { get; set; }

	private int _freezeFramesRemaining = 0;

	protected override void OnStart()
	{
		Camera ??= Components.Get<CameraComponent>();

	}

	protected override void OnUpdate()
	{
		if ( _freezeFramesRemaining == 1 )
		{
			UnfreezeCamera();
		}
		_freezeFramesRemaining = Math.Max( _freezeFramesRemaining - 1, 0 );
	}

	private void UnfreezeCamera()
	{
		if ( RenderTags is not null )
		{
			foreach ( var tag in RenderTags.TryGetAll() )
			{
				Camera.RenderTags.Remove( tag );
			}
		}
		if ( ExcludeTags is not null )
		{
			foreach ( var tag in ExcludeTags.TryGetAll() )
			{
				Camera.RenderExcludeTags.Remove( tag );
			}
		}
		Camera.ClearFlags |= ClearFlags.Color;
	}

	public void OnAfterOriginShift( Vector3 offset )
	{
		if ( RenderTags is not null )
		{
			Camera.RenderTags.Add( RenderTags );
		}
		if ( ExcludeTags is not null )
		{
			Camera.RenderExcludeTags.Add( ExcludeTags );
		}
		Camera.ClearFlags ^= ClearFlags.Color;
		_freezeFramesRemaining = OriginShiftFreezeFrames;
		Transform.Position += offset;
	}
}
