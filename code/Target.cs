using Sandbox;
using System;

public struct Target : IEquatable<Target>
{
	public Target( GameObject go )
	{
		GameObject = go;
	}

	public GameObject GameObject { get; set; }
	public Vector3 AbsolutePosition
	{
		get
		{
			return GameObject?.GetAbsolutePosition()
				?? _absolutePosition;
		}
		set
		{
			_absolutePosition = value;
			GameObject = null;
		}
	}
	private Vector3 _absolutePosition;

	public Vector3 RelativePosition
	{
		get
		{
			return GameObject?.GetAbsolutePosition()
				?? AbsolutePosition.ToRelativePosition();
		}
		set
		{
			AbsolutePosition = value.ToAbsolutePosition();
			GameObject = null;
		}
	}

	public static Target FromAbsolutePosition( Vector3 absolutePos )
	{
		return new Target() { AbsolutePosition = absolutePos };
	}

	public static Target FromRelativePosition( Vector3 relativePos )
	{
		return new Target() { RelativePosition = relativePos };
	}

	public bool Equals( Target other )
	{
		if ( GameObject is not null || other.GameObject is not null )
		{
			return GameObject == other.GameObject;
		}	
		return AbsolutePosition == other.AbsolutePosition;
	}

	public static bool operator ==(Target t1, Target t2 )
	{
		return t1.Equals( t2 );
	}
	
	public static bool operator !=(Target t1, Target t2 )
	{
		return !t1.Equals( t2 );
	}

	public override bool Equals( object obj )
	{
		return obj is Target target && Equals( target );
	}

	public override int GetHashCode()
	{
		if ( GameObject is not null )
		{
			return GameObject.GetHashCode();
		}
		return AbsolutePosition.GetHashCode();
	}

	public override string ToString()
	{
		if ( GameObject is not null )
		{
			return $"({GameObject.Name}) abs:{AbsolutePosition} rel:{RelativePosition}";
		}
		return $"abs:{AbsolutePosition} rel:{RelativePosition}";
	}
}
