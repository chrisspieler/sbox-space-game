using Sandbox;

public sealed class CollisionBubbler : Component, Component.ICollisionListener
{
	public void OnCollisionStart( Collision other ) 
	{
		var parent = GameObject.Parent;
		while ( parent is not null )
		{
			var listener = parent.Components.Get<ICollisionListener>();
			if ( listener is not null )
			{
				listener.OnCollisionStart( other );
				break;
			}
			parent = parent.Parent;
		}
	}

	public void OnCollisionStop( CollisionStop other ) 
	{
		var parent = GameObject.Parent;
		while ( parent is not null )
		{
			var listener = parent.Components.Get<ICollisionListener>();
			if ( listener is not null )
			{
				listener.OnCollisionStop( other );
				break;
			}
			parent = parent.Parent;
		}
	}

	public void OnCollisionUpdate( Collision other ) 
	{
		var parent = GameObject.Parent;
		while ( parent is not null )
		{
			var listener = parent.Components.Get<ICollisionListener>();
			if ( listener is not null )
			{
				listener.OnCollisionUpdate( other );
				break;
			}
			parent = parent.Parent;
		}
	}
}
