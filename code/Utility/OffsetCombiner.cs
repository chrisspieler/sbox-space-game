using Sandbox;

public sealed class OffsetCombiner : Component
{
	private Transform _lastTransform;

	protected override void OnUpdate()
	{
		Transform.World = GetBasis();
		AddOffsets();
	}

	protected override void OnEnabled()
	{
		_lastTransform = Transform.World;
	}

	private Transform GetBasis()
	{
		if ( !Components.TryGet<IBasisSource>( out var basis ) )
			return Transform.World;

		var baseTx = basis.GetBaseTransform( _lastTransform );
		_lastTransform = baseTx;
		return baseTx;
	}

	private void AddOffsets()
	{
		foreach ( var offsetSource in Components.GetAll<IOffsetSource>() )
		{
			var offset = offsetSource.GetOffset();
			Transform.Position += offset.Position;
			Transform.Rotation *= offset.Rotation;
		}
	}
}
