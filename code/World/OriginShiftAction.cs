using System;

namespace Sandbox;

[Group( "World")]
public class OriginShiftAction : Component, IOriginShiftListener
{
	[Property] public Action<Vector3> OnBeforeOriginShifted { get; set; }
	[Property] public Action<Vector3> OnAfterOriginShifted { get; set; }

	public void OnBeforeOriginShift( Vector3 offset ) => OnBeforeOriginShifted?.Invoke( offset );
	public void OnAfterOriginShift( Vector3 offset ) => OnAfterOriginShifted?.Invoke( offset );
}
