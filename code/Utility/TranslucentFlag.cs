using Sandbox;

public sealed class TranslucentFlag : Component, Component.ExecuteInEditor
{

	[Property] public ModelRenderer Renderer 
	{
		get => _renderer;
		set
		{
			_renderer = value;
			MakeTranslucent();
		}
	}
	private ModelRenderer _renderer;

	protected override void OnEnabled()
	{
		MakeTranslucent();
	}

	public void MakeTranslucent()
	{
		if ( Renderer?.SceneObject is null )
		{
			return;
		}
		Renderer.SceneObject.Flags.IsTranslucent = true;
	}
}
