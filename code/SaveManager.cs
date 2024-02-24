using Sandbox;

public sealed class SaveManager : Component
{
	protected override void OnStart()
	{
		// TODO: Load career data from disk.
		Career.Active = new Career()
		{
			Money = 250
		};
		Log.Info( $"Career credits: {Career.Active.Money}" );
	}
}
