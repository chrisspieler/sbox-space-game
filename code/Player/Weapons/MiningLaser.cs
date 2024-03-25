using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class MiningLaser : Weapon, IDestructionListener
{
	[Property] public List<BeamWeapon> WeaponInstances { get; set; } = new();
	[Property, Range(0.5f, 100f, 0.5f)] public float TickDamage
	{
		get => WeaponInstances?.Any() == true 
			? WeaponInstances.Average( w => w.TickDamage )
			: 0f;
		set => SetTickDamage( value );
	}
	[Property, Range(0.05f, 2f, 0.05f)] public float TickInterval
	{
		get => WeaponInstances?.Any() == true
			? WeaponInstances.Average( w => w.TickInterval )
			: 0f;
		set => SetTickInterval( value );
	}
	[Property] public Color LaserTint 
	{
		get => WeaponInstances.FirstOrDefault()?.LaserTint ?? default;
		set => SetLaserColor( value );
	}

	[Property] public LaserGradient Gradient { get; set; }

	public void SetTickDamage( float value )
	{
		if ( !WeaponInstances.Any() )
			return;

		foreach ( var weapon in WeaponInstances )
		{
			weapon.TickDamage = value;
		}
	}

	public void SetTickInterval( float value )
	{
		if ( !WeaponInstances.Any() )
			return;

		foreach ( var weapon in WeaponInstances )
		{
			weapon.TickInterval = value;
		}
	}

	public void SetLaserColor( Color value )
	{
		if ( !WeaponInstances.Any() )
			return;

		foreach ( var weapon in WeaponInstances )
		{
			weapon.LaserTint = value;
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		var weaponCount = WeaponInstances.Count;
		for( int i = 0; i < weaponCount; i++ )
		{
			var weapon = WeaponInstances[i];
			weapon.Damage.TickPhaseOffset = i * 1f / weaponCount;
			weapon.OnTargetChanged += OnTargetChanged;
			weapon.Damage.AfterDamageTick += OnAfterDamage;
		}
	}

	private void OnTargetChanged( BeamWeapon sender, GameObject newTarget )
	{
		if ( newTarget is not null )
		{
			ScreenEffects.AddScreenShake( 0.3f.LerpTo( 0.5f, sender.LaserPower ), 0.5f );
		}
	}

	private void OnAfterDamage( DamageInfo damage )
	{
		ScreenEffects.AddScreenShake( 0.2f, 0.5f );
	}

	protected override void OnPrefabInstanceAdded( GameObject instance )
	{
		WeaponInstances.Add( instance.Components.Get<BeamWeapon>() );
	}

	protected override void OnUpdate()
	{
		if ( Scene.TimeScale == 0f )
			return;

		SwivelWeapons();
		if ( !ShouldFire )
		{
			StopFiringAllWeapons();
			return;
		}
		FireAllWeapons();
		UpdateLaserTint();
	}

	private void UpdateLaserTint()
	{
		if ( Gradient is not null )
		{
			LaserTint = Gradient.GetColor();
		}
	}

	private void FireAllWeapons()
	{
		foreach( var weapon in WeaponInstances )
		{
			weapon.ShouldFire = !IsBlockedBySafety( weapon );
		}
	}

	private bool IsBlockedBySafety( BeamWeapon weapon )
	{
		var targetPos = TargetPosition ?? Scene.Camera.MouseToWorld();
		return weapon.Tracer.Transform.Position.Distance( targetPos ) > 150f
			&& WouldHitSelf( weapon, targetPos );
	}

	private bool WouldHitSelf( BeamWeapon weapon, Vector3 aimPos )
	{
		var startPos = weapon.Tracer.Transform.Position.WithZ( 0f );
		return Scene.Trace
			.Ray( startPos, aimPos )
			.WithAllTags( "player", "ship")
			.Run()
			.Hit;
	}

	public void OnMakeDebris()
	{
		StopFiringAllWeapons();
	}

	private void StopFiringAllWeapons()
	{
		foreach ( var weapon in WeaponInstances )
		{
			weapon.ShouldFire = false;
		}
	}
}
