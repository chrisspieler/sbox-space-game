using Sandbox;
using System;

public struct Target : IEquatable<Target>, IValid
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
			if ( GameObject.IsValid() )
				return GameObject.GetAbsolutePosition();

			return _absolutePosition;
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
			if ( GameObject.IsValid() )
				return GameObject.Transform.Position;

			return AbsolutePosition.ToRelativePosition();
		}
		set
		{
			AbsolutePosition = value.ToAbsolutePosition();
			GameObject = null;
		}
	}

	public float GetMetersFromFloatingOrigin()
	{
		var scene = Game.ActiveScene;
		if ( scene is null )
			return 0f;

		var originSystem = scene.GetSystem<FloatingOriginSystem>();
		var originObject = originSystem.Origin.IsValid()
			? originSystem.Origin.GameObject
			: scene.Camera.GameObject; // If the player is dead, get distance from camera instead.
		var distance = AbsolutePosition.Distance( originObject.GetAbsolutePosition() );
		return Metric.InchesToMeters( distance );
	}

	/// <summary>
	/// Returns true unless <see cref="GameObject"/> is set and is no longer valid.
	/// </summary>
	public bool IsValid => GameObject is null || GameObject.IsValid();

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

	public static implicit operator Target( Vector3 relativePos )
		=> FromRelativePosition( relativePos );

	public static implicit operator Target( GameObject gameObject )
		=> new Target( gameObject );

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
