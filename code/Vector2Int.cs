using System;

namespace Sandbox;

public struct Vector2Int : IEquatable<Vector2Int>
{
	public int X { get; set; }
	public int Y { get; set; }

	public static readonly Vector2Int Zero = new( 0 );
	public static readonly Vector2Int One = new( 1 );

	public Vector2Int( int value )
	{
		X = value;
		Y = value;
	}

	public Vector2Int( int x, int y )
	{
		X = x;
		Y = y;
	}

	public float Length()
		=> (float)Math.Sqrt( X * X + Y * Y );

	public int RectDistance( Vector2Int other )
		=> Math.Abs( X - other.X ) + Math.Abs( Y - other.Y );

	public Vector2Int Normalize()
	{
		if ( X == 0 && Y == 0 )
			return Zero;
		if ( X == 0 )
			return new( 0, Y / Math.Abs( Y ) );
		if ( Y == 0 )
			return new( X / Math.Abs( X ), 0 );
		return new( X / Math.Abs( X ), Y / Math.Abs( Y ) );
	}

	public override string ToString()
		=> $"({X}, {Y})";

	public bool Equals( Vector2Int other )
		=> other.X == X && other.Y == Y;

	public override bool Equals( object obj )
		=> obj is Vector2Int other && Equals( other );

	public override int GetHashCode()
		=> HashCode.Combine( X, Y );

	public static bool operator ==( Vector2Int a, Vector2Int b )
		=> a.Equals( b );

	public static bool operator !=( Vector2Int a, Vector2Int b )
		=> !a.Equals( b );

	public static Vector2Int operator +( Vector2Int a, Vector2Int b )
		=> new( a.X + b.X, a.Y + b.Y );

	public static Vector2Int operator -( Vector2Int a, Vector2Int b )
		=> new( a.X - b.X, a.Y - b.Y );

	public static Vector2Int operator *( Vector2Int a, int b )
		=> new( a.X * b, a.Y * b );

	public static Vector2Int operator /( Vector2Int a, int b )
		=> new( a.X / b, a.Y / b );

	public static implicit operator Vector2Int( Vector3 v )
		=> new( (int)v.x, (int)v.y );

	public static implicit operator Vector3( Vector2Int v )
		=> new( v.X, v.Y, 0 );

	public static implicit operator Vector2Int( (int x, int y) t )
		=> new( t.x, t.y );
}
