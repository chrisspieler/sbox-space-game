using Sandbox;
using System;
using System.Linq;

public sealed class NavBlocker : Component
{
	[Property] public GameObject Target { get; set; }
	private GameObject _navBlockInstance;

	protected override void OnEnabled()
	{
		_navBlockInstance.SetEnabled( true );
	}

	protected override void OnDisabled()
	{
		_navBlockInstance.SetEnabled( false );
	}

	protected override void OnDestroy()
	{
		_navBlockInstance?.Destroy();
	}

	protected override void OnValidate()
	{
		Target ??= GameObject;
	}

	protected override void OnUpdate()
	{
		if ( !Target.IsValid() )
		{
			GameObject.Destroy();
			return;
		}

		if ( !_navBlockInstance.IsValid() )
		{
			_navBlockInstance ??= CreateNavBlocker();
		}
		_navBlockInstance.Transform.Rotation = Target.Transform.Rotation;
		_navBlockInstance.Transform.Scale = Target.Transform.Scale;
	}

	private GameObject CreateNavBlocker()
	{
		var bounds = Target.GetBounds();
		var blockerGo = new GameObject();
		AddToNavBlockerFolder( blockerGo );
		blockerGo.Name = $"({GameObject.Name}) nav blocker";
		blockerGo.Tags.Add( "nav_block" );
		blockerGo.Tags.Add( "no_chunk" );
		blockerGo.Transform.Scale = Target.Transform.Scale;
		var follower = blockerGo.Components.Create<Follower>();
		follower.Target = Target;
		var collider = blockerGo.Components.Create<BoxCollider>();
		collider.Center = bounds.Center;
		collider.Scale = bounds.Size;
		return blockerGo;
	}

	public const string NAV_BLOCKER_FOLDER_NAME = "Nav Blockers";

	private void AddToNavBlockerFolder( GameObject go )
	{
		if ( !Scene.IsValid() || !go.IsValid() )
			return;

		var navBlockerFolder = Scene.Directory.FindByName( NAV_BLOCKER_FOLDER_NAME )
			.FirstOrDefault()
			?? CreateNavBlockerFolder();
		var oldTx = go.Transform.World;
		go.Parent = navBlockerFolder;
		go.Transform.World = oldTx;
	}

	private GameObject CreateNavBlockerFolder()
	{
		var navBlockerFolder = new GameObject( true, NAV_BLOCKER_FOLDER_NAME );
		navBlockerFolder.Tags.Add( "no_chunk" );
		return navBlockerFolder;
	}
}
