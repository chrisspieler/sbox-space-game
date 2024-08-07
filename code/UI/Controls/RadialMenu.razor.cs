﻿using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System;

public partial class RadialMenu : Panel
{
	public class Item
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public string Subtitle { get; set; }
		public string RootClass { get; set; }
		public Action OnSelected { get; set; }
	}

	public virtual string Button => "quick_menu";
	public virtual float OpenDelay => 0f;

	public List<Item> Items { get; private set; } = new();
	public Item HoveredItem { get; private set; }
	public Panel ItemContainer { get; set; }
	public string Name => HoveredItem?.Name;
	public string Description => HoveredItem?.Description;
	public string Subtitle => HoveredItem?.Subtitle;
	public Panel Dot { get; set; }
	public Image Arrow { get; set; }
	public bool IsOpen { get; private set; }

	private TimeSince TimeSinceButtonHeld { get; set; }
	private TimeSince LastCloseTime { get; set; }
	private Vector2 VirtualMouse { get; set; }
	private bool HasPressedButton { get; set; }

	public Item AddItem( string name, string description, string icon, Action callback )
	{
		var item = new Item
		{
			Name = name,
			Description = description,
			Icon = icon,
			OnSelected = callback
		};

		Items.Add( item );

		return item;
	}

	public void BuildInput()
	{
		if ( ItemContainer == null ) return;

		var shouldOpen = ShouldOpen();

		if ( Input.Pressed( Button ) && shouldOpen )
		{
			TimeSinceButtonHeld = 0f;
			HasPressedButton = true;
			VirtualMouse = Screen.Size * 0.5f;
		}

		if ( HasPressedButton && Input.Down( Button ) && shouldOpen )
		{
			if ( !IsOpen && TimeSinceButtonHeld > OpenDelay )
			{
				Items.Clear();
				Populate();
				IsOpen = true;
			}
		}

		if ( Input.Released( Button ) || !shouldOpen )
		{
			HasPressedButton = false;
			IsOpen = false;
		}

		if ( IsOpen )
		{
			var mouseDelta = Input.MouseDelta;

			VirtualMouse += mouseDelta;
			VirtualMouse = VirtualMouse.Clamp( Vector2.Zero, Screen.Size );

			var lx = VirtualMouse.x - Box.Left;
			var ly = VirtualMouse.y - Box.Top;

			Item closestItem = null;
			var closestDistance = 0f;

			if ( VirtualMouse.Distance( Screen.Size * 0.5f ) >= Box.Rect.Size.x * 0.1f )
			{
				var children = ItemContainer.ChildrenOfType<RadialMenuItem>();

				foreach ( var child in children )
				{
					var distance = child.Box.Rect.Center.Distance( VirtualMouse );

					if ( closestItem == null || distance < closestDistance )
					{
						closestDistance = distance;
						closestItem = child.Item;
					}
				}
			}

			HoveredItem = closestItem;

			Dot.Style.Left = Length.Pixels( lx * ScaleFromScreen );
			Dot.Style.Top = Length.Pixels( ly * ScaleFromScreen );

			if ( HoveredItem != null )
			{
				var index = Items.IndexOf( HoveredItem );
				var theta = (index * 2f * MathF.PI / Items.Count) - MathF.PI;
				var degrees = (180f / MathF.PI) * theta;

				var tx = new PanelTransform();
				tx.AddRotation( 0f, 0f, 180f - degrees );
				tx.AddTranslateY( Length.Percent( -30f ) );
				Arrow.Style.Transform = tx;
			}

			if ( HoveredItem != null && Input.Down( "attack1" ) )
			{
				HoveredItem.OnSelected?.Invoke();
				HasPressedButton = false;
				LastCloseTime = 0f;
				IsOpen = false;
			}

			Input.AnalogLook = Angles.Zero;
		}

		if ( IsOpen || LastCloseTime < 0.1f )
		{
			Input.Clear( "attack1" );
			Input.Clear( "attack2" );
		}

		if ( IsOpen )
		{
			Input.ClearActions();
			Input.AnalogMove = Vector3.Zero;
		}
	}

	public virtual void Populate()
	{

	}

	public override void Tick()
	{
		foreach ( var child in ItemContainer.ChildrenOfType<RadialMenuItem>() )
		{
			child.IsSelected = (HoveredItem == child.Item);

			var maxItemScale = 1.5f;
			var minItemScale = 0.8f;
			var distanceToMouse = child.Box.Rect.Center.Distance( VirtualMouse );
			var distanceToScale = distanceToMouse.Remap( 0f, child.Box.Rect.Size.Length * 4, maxItemScale, minItemScale ).Clamp( minItemScale, maxItemScale );

			var tx = new PanelTransform();
			tx.AddScale( distanceToScale );

			child.Style.Transform = tx;
			child.Style.ZIndex = child.IsSelected ? 2 : 0;

			child.SetClass( "selected", child.IsSelected );
		}

		base.Tick();
	}

	protected override void OnParametersSet()
	{
		Items.Clear();
		Populate();

		base.OnParametersSet();
	}

	protected virtual bool ShouldOpen()
	{
		return true;
	}

	protected override int BuildHash()
	{
		return HashCode.Combine( Name, Description, Items.Count, IsOpen, HoveredItem );
	}

	protected override void FinalLayoutChildren( Vector2 offset )
	{
		var radius = Box.Rect.Size.x * 0.5f;
		var center = Box.Rect.WithoutPosition.Center;

		for ( var i = 0; i < ItemContainer.ChildrenCount; i++ )
		{
			if ( ItemContainer.GetChild( i ) is RadialMenuItem child )
			{
				var theta = (i * 2f * Math.PI / ItemContainer.ChildrenCount) - Math.PI;
				var x = (float)Math.Sin( theta ) * radius;
				var y = (float)Math.Cos( theta ) * radius;

				child.Style.Left = Length.Pixels( (center.x + x) * ScaleFromScreen );
				child.Style.Top = Length.Pixels( (center.y + y) * ScaleFromScreen );
			}
		}

		base.FinalLayoutChildren( offset );
	}

	private string GetRootClass()
	{
		return $"{(!IsOpen ? "hidden" : string.Empty)} {HoveredItem?.RootClass ?? string.Empty}";
	}
}
