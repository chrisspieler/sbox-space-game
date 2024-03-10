using Sandbox;

public sealed class BeamWeapon : Component
{
	[Property] public BeamTrace Tracer { get; set; }
	[Property] public DamageOverTime Damage { get; set; }
	[Property] public LaserBeam LaserEffect { get; set; }
	[Property] public ImpactEffect Impact { get; set; }
	[Property] public bool ShouldFire { get; set; } = false;

	protected override void OnEnabled()
	{
		if ( Tracer is not null )
		{
			Tracer.OnHit += Hit;
			Tracer.OnMiss += Miss;
		}
		if ( Damage is not null )
		{
			Damage.AfterDamageTick += AfterDamage;
		}
		if ( LaserEffect is not null )
		{
			LaserEffect.Target = Impact.GameObject;
		}
	}

	protected override void OnDisabled()
	{
		if ( Tracer is not null )
		{
			Tracer.OnHit -= Hit;
			Tracer.OnMiss -= Miss;
		}
		if ( Damage is not null )
		{
			Damage.AfterDamageTick -= AfterDamage;
		}
	}

	protected override void OnUpdate()
	{
		Tracer.SetEnabled( ShouldFire );
		LaserEffect.SetEnabled( ShouldFire );
	}
	
	private void Hit( TraceResult tr )
	{
		Damage.Target = tr.GameObject;
		Damage.Enabled = true;
		UpdateImpact( tr );
	}

	private void Miss( TraceResult tr )
	{
		Damage.Target = null;
		Damage.Enabled = false;
		UpdateImpact( tr );
	}

	private void UpdateImpact( TraceResult tr )
	{
		if ( Impact is null )
			return;

		Impact.Target = tr.GameObject;
		Impact.Transform.Position = tr.EndPosition;
		Impact.Transform.Rotation = Rotation.LookAt( tr.Normal );
	}

	private void AfterDamage( TargetedDamageInfo damage )
	{

	}
}
