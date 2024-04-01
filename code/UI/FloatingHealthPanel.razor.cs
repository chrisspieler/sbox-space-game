using Sandbox;

public partial class FloatingHealthPanel : PanelComponent
{
	[Property] public GameObject Target { get; set; }
	public IHealth Health { get; set; }

	protected override int BuildHash() => System.HashCode.Combine( Target, Health, Health?.CurrentHealth, Health?.MaxHealth );

	private float GetHealthPercent()
	{
		if ( Health is null )
			return 0f;

		return (Health.CurrentHealth / Health.MaxHealth) * 100f;
	}
}
