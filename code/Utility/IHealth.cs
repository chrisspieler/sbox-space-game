using Sandbox;
using System;

public interface IHealth
{
	float MaxHealth { get; set; }
	float CurrentHealth { get; set; }
	Action<DamageInfo> OnDamaged { get; set; }
}
