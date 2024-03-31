using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

public sealed class RopePath : Component
{
	[Property] public ParticleSystem RopeAsset { get; set; } = ParticleSystem.Load( "particles/slack_rope.vpcf" );

	public IEnumerable<RopeSegment> Segments => _ropeSegments;
	private readonly List<RopeSegment> _ropeSegments = new();
	public bool IsEmpty => !_ropeSegments.Any();

	protected override void OnUpdate()
	{
		if ( RopeTester.RopeDebug )
		{
			Gizmo.Draw.Color = Color.Yellow;
			foreach ( var segment in _ropeSegments )
			{
				var position = segment.GameObject.Transform.Position + Vector3.Up * 3f;
				Gizmo.Draw.Text( $"index: {_ropeSegments.IndexOf( segment )}", new Transform( position ), "Consolas" );
			}
		}

		UpdatePhysics();
	}

	private void UpdatePhysics()
	{
		MergeOnUnwind();
		SplitOnCorner();
		for ( int i = 0; i < _ropeSegments.Count; i++ )
		{
			_ropeSegments[i].Tension = Math.Min( i + 1, 5 );
		}
	}

	/// <summary>
	/// Wrap the rope around any solid obstacles it collides with. Only the first segment 
	/// (nearest the player) will respond to collisions. This assumes that the solid 
	/// parts of the world will not move around.
	/// </summary>
	private void SplitOnCorner()
	{
		if ( !_ropeSegments.Any() )
		{
			return;
		}
		var firstSegment = _ropeSegments.First();
		var startPos = firstSegment.Transform.Position;
		var endPos = firstSegment.NextPoint.Transform.Position;
		// Trace a ray from the source of the rope to either the end of the
		// rope or the start of the second rope segment.
		var tr = Scene.Trace
			.Ray( startPos, endPos )
			.WithAllTags( "solid" )
			.Run();
		// If the ray hits something near where the second rope segment would begin,
		// don't split the rope. This is to prevent lots of tiny rope segments from
		// being created (e.g. when the rope is wrapped around a sphere).
		var isHitNearNextPoint = endPos.Distance( tr.EndPosition ) > 5f;
		if ( tr.Hit && isHitNearNextPoint )
		{
			// Create the new rope point offset from the hit position so that the
			// rope doesn't intersect the solid object so much.
			var offset = -tr.Direction * 0.5f;
			SplitSegment( firstSegment, tr.HitPosition + offset );
		}
	}

	/// <summary>
	/// Merges the first two rope segments in to one if it is possible to do so without the
	/// rope going through a solid object. 
	/// </summary>
	private void MergeOnUnwind()
	{
		if ( _ropeSegments.Count < 2 )
		{
			return;
		}
		var firstSegment = _ropeSegments.First();
		var nextSegment = _ropeSegments[1];
		// Test whether we can reach the next rope point without hitting a wall.
		var hitNextPoint = Scene.Trace
			.Ray( firstSegment.Transform.Position, nextSegment.NextPoint.Transform.Position )
			.WithAllTags( "solid" )
			.Run()
			.Hit;
		// Get a position on the first rope segment that is 5 units from the end of the segment.
		var startOffset = firstSegment.GetDirectionToNextPoint() * (firstSegment.GetDistanceToNextPoint() - 5f);
		var startPos = firstSegment.Transform.Position + startOffset;
		// Get a position on the second rope segment that is 5 units away from the start of the segment.
		var endOffset = nextSegment.GetDirectionToNextPoint() * 5f;
		var endPos = nextSegment.Transform.Position + endOffset;
		// Find a direction from second to the first rope segment.
		var direction = (startPos - endPos).Normal;
		// Adjust the end position so that the ray should collide with a wall if the first
		// segment is around the corner from the second.
		endPos -= direction * 1f;
		var ray = new Ray( endPos, direction );
		// See whether the ray hit a corner we're wrapped around.
		var cornerTr = Scene.Trace
			.Ray( ray, 20f )
			.WithAllTags( "solid" )
			.Run();

		// Draw a red line to show the trace we do for the confusing corner detection logic.
		if ( RopeTester.RopeDebug )
		{
			Gizmo.Draw.Color = cornerTr.Hit
				? Color.Red : Color.Blue;
			Gizmo.Draw.IgnoreDepth = true;
			Gizmo.Draw.Line( cornerTr.StartPosition, cornerTr.EndPosition );
		}

		if ( !hitNextPoint && !cornerTr.Hit )
		{
			MergeWithPrevious( nextSegment );
		}
	}

	public void Clear()
	{
		var segments = _ropeSegments.ToArray();
		foreach ( var segment in segments )
		{
			RemoveSegment( segment );
		}
		// Clean up the last rope point, which has no RopeSegment component.
		var lastPoint = segments.LastOrDefault()?.NextPoint;
		if ( lastPoint.IsValid() && lastPoint.Tags.Has( "rope" ) )
		{
			lastPoint.Destroy();
		}
	}

	/// <summary>
	/// Find this path's closest RopeSegment to the given position, or null if
	/// this RopePath is empty.
	/// </summary>
	public RopeSegment GetNearestSegment( Vector3 position )
	{
		if ( !Segments.Any() )
		{
			return null;
		}
		return Segments
			.Select( s => (Segment: s, Distance: s.GetMidpoint().Distance( position )) )
			.OrderBy( s => s.Distance )
			.First()
			.Segment;
	}

	private void RemoveSegment( RopeSegment segment )
	{
		// If a GameObject was created just to be rope, clean it up with the rope.
		if ( segment.Tags.Has( "rope" ) )
		{
			segment.GameObject.Destroy();
		}
		else
		{
			segment.Destroy();
		}
		segment.RopeSystem?.Destroy();
		_ropeSegments.Remove( segment );
	}

	/// <summary>
	/// Create an empty game object meant to hold or be pointed to by a RopeSegment.
	/// </summary>
	private GameObject CreateRopePointObject( Vector3 position )
	{
		var pointNum = _ropeSegments.Count;
		var go = new GameObject( true, $"Rope Point {pointNum}" );
		go.Tags.Add( "rope" );
		go.Transform.Position = position;
		return go;
	}

	/// <summary>
	/// Create a RopeSegment component attached to one GameObject and pointing to another.
	/// </summary>
	private RopeSegment CreateRopeSegment( GameObject currentPoint, GameObject nextPoint )
	{
		var segment = currentPoint.Components.Create<RopeSegment>();
		segment.RopeAsset = RopeAsset;
		segment.NextPoint = nextPoint;
		return segment;
	}

	/// <summary>
	/// Append to the rope a new RopeSegment at the given position.
	/// </summary>
	public void ExtendRope( Vector3 nextPosition )
	{
		if ( !_ropeSegments.Any() )
		{
			MakeRopeLine( GameObject, nextPosition );
			return;
		}
		var lastSegment = _ropeSegments.Last();
		lastSegment.NextPoint ??= CreateRopePointObject( nextPosition );
		var nextPoint = CreateRopePointObject( nextPosition );
		var segment = CreateRopeSegment( lastSegment.NextPoint, nextPoint );
		_ropeSegments.Add( segment );
	}

	/// <summary>
	/// Clear the entire rope and create a single RopeSegment from the given position to the given point.
	/// </summary>
	public void MakeRopeLine( GameObject from, Vector3 to )
	{
		Clear();
		var ropePoint = CreateRopePointObject( to );
		var segment = CreateRopeSegment( GameObject, ropePoint );
		_ropeSegments.Add( segment );
	}

	/// <summary>
	/// Create a new RopeSegment in the middle of an existing segment.
	/// </summary>
	public void SplitSegment( RopeSegment segment, Vector3 splitPoint )
	{
		var oldEnd = segment.NextPoint;
		if ( !oldEnd.IsValid() )
		{
			throw new Exception( "Cannot split a segment with no end point." );
		}
		var startIndex = _ropeSegments.IndexOf( segment );
		var newPoint = CreateRopePointObject( splitPoint );
		segment.NextPoint = newPoint;
		segment.RopeLength = segment.GetDistanceToNextPoint();
		var newSegment = CreateRopeSegment( newPoint, oldEnd );
		_ropeSegments.Insert( startIndex + 1, newSegment );
	}

	/// <summary>
	/// Delete this RopeSegment, connecting the previous segment to the next segment.
	/// </summary>
	public void MergeWithPrevious( RopeSegment segment )
	{
		var index = _ropeSegments.IndexOf( segment );
		if ( index == 0 ) return;
		var previousSegment = _ropeSegments[index - 1];
		RemoveSegment( segment );
		previousSegment.NextPoint = segment.NextPoint;
	}
}
