using Sandbox;

public sealed class Thruster : Component
{
	[Property] public GameObject EffectPrefab { get; set; }
	private GameObject _effectInstance { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();
	}
}
