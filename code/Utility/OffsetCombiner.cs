using Sandbox;

public sealed class OffsetCombiner : Component
{
	protected override void OnUpdate()
	{
		Transform.World = GetBasis();
		AddOffsets();
	}

	private Transform GetBasis()
	{
		if ( !Components.TryGet<IBasisSource>( out var basis ) )
			return Transform.World;

		return basis.GetBaseTransform();
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
