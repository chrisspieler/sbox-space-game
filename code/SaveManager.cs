using Sandbox;

public sealed class SaveManager : Component
{
	[Property] public Scenario DefaultScenario { get; set; }
	[Property] public GameObject ShipPrefab { get; set; }

	protected override void OnStart()
	{
		// TODO: Load in-progress career data from disk.
		InitializeScenario( DefaultScenario );
	}

	private void InitializeScenario( Scenario scenario )
	{
		Career.Active = new Career()
		{
			Money = scenario.Money
		};
		foreach( var upgrade in scenario.Upgrades )
		{
			Career.AddUpgrade( upgrade );
		}
		ShipController.Respawn();
	}
}
