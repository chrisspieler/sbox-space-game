using Sandbox;
using System;

public sealed class DamageOverTime : Component
{
	/// <summary>
	/// Invoked after damage has been applied to <see cref="Target"/>
	/// </summary>
	[Property] public Action<TargetedDamageInfo> AfterDamageTick { get; set; }

	[Property] public GameObject Owner { get; set; }
	[Property] public GameObject Weapon { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public Vector3? HitPosition { get; set; }
	[Property] public float TickDamage { get; set; } = 2.5f;
	[Property] public float TickInterval { get; set; } = 0.2f;
	/// <summary>
	/// Offsets the initial damage tick progress by a fraction of <see cref="TickInterval"/>.
	/// This is useful for staggering damage ticks between weapons so that they don't all 
	/// tick at the same time.
	/// </summary>
	[Property, Range( 0f, 1f )] public float TickPhaseOffset { get; set; } = 0f;

	private TimeUntil _untilDamageTick;

	protected override void OnEnabled()
	{
		_untilDamageTick += TickInterval - TickInterval * TickPhaseOffset;
	}

	protected override void OnUpdate()
	{
		if ( !Target.IsValid() || !_untilDamageTick )
			return;

		_untilDamageTick = TickInterval;
		var damage = GetDamageInfo();
		Target.DoDamage( damage );
		AfterDamageTick?.Invoke( damage );
	}

	private TargetedDamageInfo GetDamageInfo()
	{
		var position = HitPosition
			?? Target?.Transform?.Position
			?? Transform.Position;
		return new TargetedDamageInfo()
		{
			Attacker = Owner,
			Weapon = Weapon,
			Damage = TickDamage,
			Position = position,
			Target = Target
		};
	}
}
