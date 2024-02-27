using Sandbox;
using System;
using System.Threading.Tasks;

public sealed class Pickup : Component, Component.ITriggerListener
{
	[Property] public CargoItem Item { get; set; }
	[Property] public bool DisablePickup { get; set; }
	[Property] public GameObject PickupEffectPrefab { get; set; }
	[Property, Category( "Animation" )]
	public float AnimationDuration { get; set; } = 0.4f;
	[Property, Category( "Animation" )] 
	public Curve ScaleCurve { get; set; }
	[Property, Category( "Animation" )] 
	public Curve ZPositionCurve { get; set; }

	private bool _triggered;

	protected override void OnStart()
	{
		if ( Item is not null && Item.PickupPrefab is not null )
		{
			var prefabGo = Item.PickupPrefab.Clone();
			prefabGo.Parent = GameObject;
			prefabGo.Transform.Rotation = Rotation.FromYaw( Random.Shared.Float( 0f, 360f ) );
			prefabGo.BreakFromPrefab();
		}
		GameObject.BreakFromPrefab();
	}

	public void OnTriggerEnter( Collider other ) 
	{
		if ( _triggered || DisablePickup || !other.IsPlayer() )
			return;

		var cargoHold = ShipController.GetCurrent().Cargo;
		if ( !cargoHold.IsValid() || !cargoHold.TryAddItem( Item ) )
			return;

		_triggered = true;

		if ( PickupEffectPrefab is not null )
		{
			var effectGo = PickupEffectPrefab.Clone();
			effectGo.Transform.Position = Transform.Position;
		}
		_ = DoAnimation();
	}

	public void OnTriggerExit( Collider other ) { }

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
