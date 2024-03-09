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
				ResetPositionAndRotation();
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

	private Vector3 _basePosition;
	private Rotation _baseRotation;

	protected override void OnStart()
	{
		Instance = this;
	}

	public Transform GetBaseTransform()
	{
		UpdatePositionAndRotation();
		return new Transform()
			.WithPosition( _basePosition )
			.WithRotation( _baseRotation );
	}

	protected override void OnEnabled()
	{
		ResetPositionAndRotation();
	}

	private void ResetPositionAndRotation()
	{
		if ( _target is null )
			return;

		_basePosition = _target.Transform.Position;
		_baseRotation = _target.Transform.Rotation;
	}

	private void UpdatePositionAndRotation()
	{
		if ( Target?.Components?.TryGet<Rigidbody>( out var targetPhysics ) != true )
			return;

		var lerpProgress = GetSpeedFraction( targetPhysics );
		var targetPos = GetTargetPosition( targetPhysics.Velocity, lerpProgress );
		_basePosition = _basePosition.LerpTo( targetPos, PositionLerpSpeed * Time.Delta );
		var targetRot = GetTargetRotation( lerpProgress );
		_baseRotation = Rotation.Slerp( _baseRotation, targetRot, PitchLerpSpeed * Time.Delta );
	}

	private float GetSpeedFraction( Rigidbody rb )
	{
		var targetVelocity = rb.Velocity.Length;
		return MathX.LerpInverse( targetVelocity, TargetVelocityLowThreshold, TargetVelocityHighThreshold );
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
}
