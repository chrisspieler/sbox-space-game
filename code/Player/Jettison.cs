using Sandbox;
using System.Linq;

public sealed class Jettison : Component
{
	[Property] public Rigidbody SelfRigidbody { get; set; }
	[Property] public GameObject PickupPrefab { get; set; }
	[Property] public CargoHold ItemSource { get; set; }
	[Property] public GameObject EjectionSource { get; set; }
	[Property] public GameObject EffectPrefab { get; set; }
	[Property] public float EjectionSpeed { get; set; } = 30f;
	[Property] public float EjectionSpin { get; set; } = 5f;

	protected override void OnUpdate()
	{
		if ( !Input.Pressed( "jettison" ) )
			return;

		var item = ItemSource.Items.FirstOrDefault();
		if ( item is null )
			return;

		CreateEffect();
		LaunchPickup( item );
		ItemSource.RemoveItem( item );
	}

	private void CreateEffect()
	{
		if ( EffectPrefab is not null )
		{
			var effectGo = EffectPrefab.Clone();
			effectGo.Parent = EjectionSource;
			effectGo.Transform.World = EjectionSource.Transform.World.WithScale( 1f );
		}
	}

	private void LaunchGameObject( GameObject go )
	{
		var launchVelocity = EjectionSpeed * EjectionSource.Transform.Rotation.Forward;
		if ( SelfRigidbody is not null )
		{
			SelfRigidbody.Velocity -= launchVelocity * 0.3f;
			launchVelocity += SelfRigidbody.Velocity;
		}
		if ( go.Components.TryGet<Rigidbody>( out var rb ) )
		{
			// Some pickups may have damping added so that they are easier to pick up,
			// but this is undesirable for whatever we choose to jettison.
			rb.LinearDamping = 0f;
			rb.Velocity = launchVelocity;
			rb.AngularVelocity = EjectionSpin * Vector3.Random;
		}
	}

	private GameObject LaunchPickup( CargoItem item )
	{
		var pickup = Pickup.Spawn( item, EjectionSource.Transform.Position );
		// Make sure you don't pick up the item immediately after launching it.
		pickup.DisablePickup = true;
		_ = Task.DelaySeconds( 0.5f ).ContinueWith( _ =>
		{
			if ( !pickup.IsValid() )
				return;

			pickup.DisablePickup = false;
		} );
		var pickupGo = pickup.GameObject;
		LaunchGameObject( pickupGo );
		return pickupGo;
	}
}
