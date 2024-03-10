using Sandbox;

public class TargetedDamageInfo : DamageInfo
{
	/// <summary>
	/// Whatever is actually receiving the damage (e.g. an enemy or prop)
	/// </summary>
	public GameObject Target { get; set; }
}
