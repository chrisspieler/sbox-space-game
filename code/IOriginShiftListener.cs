namespace Sandbox;

public interface IOriginShiftListener
{
	void OnBeforeOriginShift( Vector3 offset ) { }
	void OnAfterOriginShift( Vector3 offset ) { }
}
