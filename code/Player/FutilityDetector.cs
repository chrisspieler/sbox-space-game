﻿using System.Linq;

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
			// Put the text ahead of the ship so that if it is drifting fast, the player can read it.
			var position = ship.Transform.Position + ship.Rigidbody.Velocity.Normal * 200f;
			ScreenManager.ShowTextPanel( "SHIP STRANDED, HOLD LEFT-CLICK ON SELF TO RESTART", position, false, 5f );
		}
	}

	private static bool IsStranded( ShipController ship )
	{
		var hasFuel = HasFuel( ship );
		var hasQtDrive = HasQtDrive( ship );
		var canGrappleBeam = CanGrappleBeam( ship );
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
			Gizmo.Draw.ScreenText( $"hasQtDrive: {hasQtDrive}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"canGrappleBeam: {canGrappleBeam}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"canJettison: {canJettison}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"isShopOpen: {isShopOpen}", position, "Consolas", 12, TextFlag.CenterTop );
			position += Vector2.Up * 25f;
			Gizmo.Draw.ScreenText( $"isDriftingTowardShop: {isDriftingTowardShop}", position, "Consolas", 12, TextFlag.CenterTop );
		}
		return !hasFuel
			&& !hasQtDrive
			&& !canGrappleBeam
			&& !canJettison
			&& !isShopOpen
			&& !isDriftingTowardShop;
	}

	private static bool HasFuel( ShipController ship )
	{
		return ship.Fuel.IsValid() && ship.Fuel.CurrentAmount >= 0.1f;
	}

	private static bool HasQtDrive( ShipController ship )
	{
		return ship.QtDrive?.Enabled == true;
	}

	private static bool CanGrappleBeam( ShipController ship )
	{
		return ship.Grapple?.Enabled == true;
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
