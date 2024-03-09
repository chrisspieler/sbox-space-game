using Sandbox;
using System;

public sealed class ShipCamera : Component, IBasisSource
{
	[Property] public OffsetCombiner Combiner { get; set; }
	[Property] public GameObject Target 
	{
		get => _target;
		set
		{
			_target = value;
			if ( Game.IsPlaying )
			{
				_shouldResetTransform = true;
			}
		}
	}
	private GameObject _target;
	[Property, Range(30f, 90f, 1f)] public float LowPitch { get; set; } = 70f;
	[Property, Range(30f, 90f, 1f)] public float HighPitch { get; set; } = 88f;
	[Property] public Vector3 LowPosition { get; set; } = new Vector3( -150, 0f, 600f );
	[Property] public Vector3 HighPosition { get; set; } = new Vector3( -200, 0f, 1500f );
	[Property, Range( 0f, 1000f, 25f )] public float TargetVelocityLowThreshold { get; set; } = 200f;
	[Property, Range( 0f, 5000f, 25f )] public float TargetVelocityHighThreshold { get; set; } = 2500f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PitchLerpSpeed { get; set; } = 1f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PositionLerpSpeed { get; set; } = 1f;

	public static ShipCamera Instance { get; private set; }
	public static ShipCamera GetCurrent() => Instance;

	private bool _shouldResetTransform;

	protected override void OnStart()
	{
		Instance = this;
	}

	public Transform GetBaseTransform( Transform lastTransform )
	{
		if ( Target?.Components?.TryGet<Rigidbody>( out var rb ) != true )
			return lastTransform;

		// On a scale from 0 - 1, how speedy is the Target?
		var speed = GetSpeedFraction( rb );
		// Given the Target's speed and direction, where would we like to be?
		var targetTx = GetTargetTransform( rb.Velocity, speed );
		// Lerp to that position
		var nextTx = GetNextTransform( lastTransform, targetTx );
		_shouldResetTransform = false;
		return nextTx;
	}

	protected override void OnEnabled()
	{
		_shouldResetTransform = true;
	}

	private float GetSpeedFraction( Rigidbody rb )
	{
		var targetVelocity = rb.Velocity.Length;
		return MathX.LerpInverse( targetVelocity, TargetVelocityLowThreshold, TargetVelocityHighThreshold );
	}

	private Transform GetTargetTransform( Vector3 velocity, float frac )
	{
		return new Transform()
			.WithPosition( GetTargetPosition( velocity, frac ) )
			.WithRotation( GetTargetRotation( frac ) );
	}

	private Vector3 GetTargetPosition( Vector3 velocity, float speedFraction )
	{
		var heightOffset = Vector3.Lerp( LowPosition, HighPosition, speedFraction );
		var nonVerticality = MathF.Abs( Vector3.Right.Dot( velocity.Normal ) );
		var travelOffset = velocity + (velocity * 0.6f) * nonVerticality;
		return Target.Transform.Position + heightOffset + travelOffset;
	}

	private Rotation GetTargetRotation( float speedFraction )
	{
		return Rotation.Lerp( Rotation.FromPitch( LowPitch ), Rotation.FromPitch( HighPitch ), speedFraction );
	}

	private Transform GetNextTransform( Transform lastTx, Transform targetTx )
	{
		return new Transform()
			.WithPosition( GetNextPosition( lastTx.Position, targetTx.Position ) )
			.WithRotation( GetNextRotation( lastTx.Rotation, targetTx.Rotation ) );
	}

	private Vector3 GetNextPosition( Vector3 lastPos, Vector3 targetPos )
	{
		return _shouldResetTransform
			? targetPos
			: lastPos.LerpTo( targetPos, PositionLerpSpeed * Time.Delta );
	}

	private Rotation GetNextRotation( Rotation lastRotation, Rotation targetRotation )
	{
		return _shouldResetTransform
			? targetRotation
			: Rotation.Slerp( lastRotation, targetRotation, PitchLerpSpeed * Time.Delta );
	}
}
