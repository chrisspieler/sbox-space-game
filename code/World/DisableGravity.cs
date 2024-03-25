using System;
using Sandbox;

public sealed class DisableGravity : Component
{
	protected override void OnStart()
	{
		Scene.PhysicsWorld.Gravity = 0f;
	}
}
