using System.Linq;

using Sandbox;

public class FutilityDetector : GameObjectSystem
{
	[ConVar( "futility_debug" )]
	public static bool Debug { get; set; }

	public FutilityDetector( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, Update, "Futility Detector" );
	}

	private TimeUntil _untilNextPopup;

	private void Update()
	{
		var ship = ShipController.GetCurrent();
		if ( !ship.IsValid() )
			return;

		if ( IsStranded( ship ) && _untilNextPopup )
		{
			_untilNextPopup = 5f;
			ScreenManager.ShowTextPanel( "SHIP STRANDED, HOLD LEFT-CLICK ON SELF TO RESTART", ship.Transform.Position, false, 5f );
		}
	}

	private static bool IsStranded( ShipController ship )
	{
		var hasFuel = HasFuel( ship );
		var canJettison = CanJettison( ship );
		var isShopOpen = ScreenManager.IsShopOpen();
		var isDriftingTowardShop = IsDriftingTowardShop( ship );
		if ( Debug )
		{
			using var _ = Gizmo.Scope( "Futility Debug" );
			Gizmo.Draw.Color = Color.Yellow;
			var position = new Vector2( Screen.Width / 2, 0f );
			Gizmo.Draw.ScreenText( "FUTILITY DETECTOR", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"hasFuel: {hasFuel}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"canJettison: {canJettison}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"isShopOpen: {isShopOpen}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"isDriftingTowardShop: {isDriftingTowardShop}", position, "Consolas", 12, TextFlag.CenterTop );
		}
		return !hasFuel
			&& !canJettison
			&& !isShopOpen
			&& !isDriftingTowardShop;
	}

	private static bool HasFuel( ShipController ship )
	{
		return ship.Fuel.IsValid() && ship.Fuel.CurrentAmount >= 0.1f;
	}

	private static bool CanJettison( ShipController ship )
	{
		return ship.Jettison?.Enabled == true
			&& ship.Cargo.IsValid()
			&& ship.Cargo.Count > 0f;
	}

	private static bool IsDriftingTowardShop( ShipController ship )
	{
		if ( ship.Rigidbody.Velocity.IsNearZeroLength )
			return false;

		var shop = Game.ActiveScene.GetAllComponents<Shop>().FirstOrDefault();
		if ( shop is null )
			return false;

		var shopDirection = ship.Transform.Position.Direction( shop.Transform.Position );
		var dotProduct = ship.Rigidbody.Velocity.Normal.Dot( shopDirection );
		return dotProduct > 0.7f;
	}
}
