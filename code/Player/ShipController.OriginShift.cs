using Sandbox;
using System.Collections.Generic;

public partial class ShipController : IOriginShiftListener
{
	private Vector3 _beforeOriginShiftVelocity;
	private Vector3 _beforeOriginShiftAngularVelocity;

	public void OnBeforeOriginShift( Vector3 offset ) 
	{
		if ( Rigidbody?.Enabled == false )
			return;

		_beforeOriginShiftVelocity = Rigidbody.Velocity;
		_beforeOriginShiftAngularVelocity = Rigidbody.AngularVelocity;
		Rigidbody.Enabled = false;
	}

	public void OnAfterOriginShift( Vector3 offset )
	{
		Rigidbody.Enabled = true;
		Rigidbody.Velocity = _beforeOriginShiftVelocity;
		Rigidbody.AngularVelocity = _beforeOriginShiftAngularVelocity;
	}
}
