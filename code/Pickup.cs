using Sandbox;
using System;
using System.Threading.Tasks;

public sealed partial class Pickup : Component, Component.ITriggerListener
{
	[Property] public CargoItem Item { get; set; }
	[Property] public bool DisablePickup { get; set; }
	[Property] public GameObject PickupEffectPrefab { get; set; }
	[Property] public SoundEvent GetSuccessSound { get; set; }
	[Property] public SoundEvent GetFailSound { get; set; }
	[Property, Category( "Animation" )]
	public float AnimationDuration { get; set; } = 0.4f;
	[Property, Category( "Animation" )] 
	public Curve ScaleCurve { get; set; }
	[Property, Category( "Animation" )] 
	public Curve ZPositionCurve { get; set; }

	private bool _triggered;

	protected override void OnStart()
	{
		CreateItemModel();
		AddRandomSpin();
		GameObject.BreakFromPrefab();
	}

	private void CreateItemModel()
	{
		var pickupPrefab = Item is not null && Item.PickupPrefab is not null
			? Item.PickupPrefab.GetPrefabScene()
			: GetDefaultPrefab();
		var prefabGo = pickupPrefab.Clone();
		prefabGo.Parent = GameObject;
		prefabGo.Transform.Rotation = Rotation.FromYaw( Random.Shared.Float( 0f, 360f ) );
		prefabGo.BreakFromPrefab();
	}

	private void AddRandomSpin()
	{
		var rigidbody = Components.Get<Rigidbody>();
		var randomYaw = Random.Shared.Float( 0.2f, 1f );
		rigidbody.AngularVelocity = Vector3.Zero.WithZ( randomYaw );
	}

	private GameObject GetDefaultPrefab()
	{
		var prefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/default_pickup_model.prefab" );
		return SceneUtility.GetPrefabScene( prefabFile );
	}

	public void OnTriggerEnter( Collider other ) 
	{
		if ( _triggered || DisablePickup || !other.IsPlayer() )
			return;

		var cargoHold = ShipController.GetCurrent().Cargo;
		if ( !cargoHold.IsValid() )
			return;

		if ( !cargoHold.TryAddItem( Item ) )
		{
			ScreenManager.ShowTextPanel( $"CARGO HOLD FULL", Transform.Position + Vector3.Up * 200f, true );
			// If the player has never before sold any cargo...
			if ( Career.Active.TotalCreditsEarned == 0 )
			{
				// ...let them know that they can sell it.
				ScreenManager.ShowTextPanel( $"SELL CARGO AT SHOP", Transform.Position + new Vector3( -50f, 0f, 200f ), true, duration: 3f );
			}
			Sound.Play( GetFailSound, Transform.Position );
			return;
		}

		_triggered = true;
		ScreenManager.ShowTextPanel( $"+1 {Item.Name}", Transform.Position + Vector3.Up * 200f, false );
		Sound.Play( GetSuccessSound, Transform.Position );
		if ( cargoHold.Count == cargoHold.MaxItems )
		{
			ScreenManager.ShowTextPanel( $"CARGO HOLD FULL", Transform.Position + new Vector3( -50f, 0f, 200f ), false, duration: 3f );
			if ( Career.Active.TotalCreditsEarned == 0 )
			{
				ScreenManager.ShowTextPanel( $"SELL CARGO AT SHOP", Transform.Position + new Vector3( -100f, 0f, 200f ), false, duration: 3f );
			}
		}

		if ( PickupEffectPrefab is not null )
		{
			var effectGo = PickupEffectPrefab.Clone();
			effectGo.Transform.Position = Transform.Position;
		}
		_ = DoAnimation();
	}

	public void OnTriggerExit( Collider other ) { }

	public static Pickup Spawn( CargoItem item, Vector3 position )
	{
		var prefabFile = ResourceLibrary.Get<PrefabFile>( "prefabs/pickup_base.prefab" );
		var prefabScene = SceneUtility.GetPrefabScene( prefabFile );
		var pickupGo = prefabScene.Clone( position.WithZ( 0f ) );
		var pickup = pickupGo.Components.Get<Pickup>();
		pickup.Item = item;
		return pickup;
	}

	private async Task DoAnimation()
	{
		TimeUntil _untilAnimationFinished = AnimationDuration;
		var startHeight = Transform.Position.z;
		while ( !_untilAnimationFinished )
		{
			var t = _untilAnimationFinished.Fraction;
			Transform.Scale = ScaleCurve.Evaluate( t );
			var currentHeight = startHeight + ZPositionCurve.Evaluate( t );
			Transform.Position = Transform.Position.WithZ( currentHeight );
			await Task.Frame();
		}
		GameObject.Destroy();
	}
}
