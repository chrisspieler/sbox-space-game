using Sandbox;

public static class ComponentExtensions
{
	public static void SetEnabled( this Component component, bool enabled )
	{
		if ( component is null )
			return;

		component.Enabled = enabled;
	}
}
