using Sandbox;

public sealed class Weapon : Component
{
	[Property] public GameObject Body { get; set; }

	protected override void OnUpdate()
	{
		UpdateBodyRotation();
		UpdateAttack();
	}

	private void UpdateAttack()
	{
		var hovered = MouseSelector.Instance?.Hovered;
		if ( hovered is null || !Input.Pressed( "fire" ) )
			return;

		if ( !hovered.Components.TryGet<IDamageable>( out var damageable, FindMode.EnabledInSelfAndDescendants ) )
			return;

		var damageInfo = new DamageInfo()
		{
			Attacker = ShipController.GetCurrent().GameObject,
			Damage = 100,
			Position = MouseSelector.Instance.Hovered.Transform.Position,
			Weapon = this.GameObject
		};
		damageable.OnDamage( damageInfo );
	}

	private void UpdateBodyRotation()
	{
		var mousePos = Scene.Camera.MouseToWorld();
		var direction = (mousePos - Body.Transform.Position).Normal;
		var yaw = Rotation.LookAt( direction ).Yaw();
		Body.Transform.Rotation = ((Angles)Body.Transform.Rotation).WithYaw( yaw );
	}
}
