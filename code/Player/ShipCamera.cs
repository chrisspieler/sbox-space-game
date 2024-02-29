using Sandbox;
using Sandbox.Utility;
using System;

public sealed class ShipCamera : Component
{
	[ConVar( "camera_shake_scale" )]
	public static float ScreenShakeScale { get; set; } = 1f;

	[Property] public GameObject Target { get; set; }
	[Property, Range(30f, 90f, 1f)] public float LowPitch { get; set; } = 70f;
	[Property, Range(30f, 90f, 1f)] public float HighPitch { get; set; } = 88f;
	[Property] public Vector3 LowPosition { get; set; } = new Vector3( -150, 0f, 600f );
	[Property] public Vector3 HighPosition { get; set; } = new Vector3( -200, 0f, 1500f );
	[Property, Range( 0f, 1000f, 25f )] public float TargetVelocityLowThreshold { get; set; } = 200f;
	[Property, Range( 0f, 5000f, 25f )] public float TargetVelocityHighThreshold { get; set; } = 2500f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PitchLerpSpeed { get; set; } = 1f;
	[Property, Range(0.1f, 5f, 0.1f)] public float PositionLerpSpeed { get; set; } = 1f;
	[Property, Range( 0f, 1f ), Category("Screen Shake")] 
	public float Trauma 
	{
		get => _trauma;
		set
		{
			_trauma = value.Clamp( 0f, 1f );
		}
	}
	private float _trauma;
	[Property, Category( "Screen Shake" )]
	public float TraumaDecayRate { get; set; } = 2f;
	[Property, Category("Screen Shake")]
	public float PitchShakeIntensity { get; set; } = 10f;
	[Property, Category( "Screen Shake" )]
	public float PitchShakeSpeed { get; set; } = 100f;
	[Property, Category( "Screen Shake" )]
	public float YawShakeIntensity { get; set; } = 10f;
	[Property, Category( "Screen Shake" )]
	public float YawShakeSpeed { get; set; } = 100f;
	[Property, Category( "Screen Shake" )]
	public float RollShakeIntensity { get; set; } = 10f;
	[Property, Category( "Screen Shake" )]
	public float RollShakeSpeed { get; set; } = 100f;

	private Rotation _baseRotation = Rotation.Identity;

	public static ShipCamera Instance { get; private set; }
	public static ShipCamera GetCurrent() => Instance;
	protected override void OnStart() => Instance = this;

	protected override void OnUpdate()
	{
		if ( Target?.Components?.TryGet<Rigidbody>( out var targetPhysics ) != true )
			return;

		var targetVelocity = targetPhysics.Velocity.Length;
		var lerpProgress = MathX.LerpInverse( targetVelocity, TargetVelocityLowThreshold, TargetVelocityHighThreshold );
		var heightOffset = Vector3.Lerp( LowPosition, HighPosition, lerpProgress );
		var nonVerticality = MathF.Abs( Vector3.Right.Dot( targetPhysics.Velocity.Normal ) );
		var travelOffset = targetPhysics.Velocity + ( targetPhysics.Velocity * 0.6f ) * nonVerticality;
		var targetPos = Target.Transform.Position + heightOffset + travelOffset;
		Transform.Position = Transform.Position.LerpTo( targetPos, PositionLerpSpeed * Time.Delta );
		var targetRot = Rotation.Lerp( Rotation.FromPitch( LowPitch ), Rotation.FromPitch( HighPitch ), lerpProgress );
		_baseRotation = Rotation.Slerp( _baseRotation, targetRot, PitchLerpSpeed * Time.Delta );
		Transform.LocalRotation = _baseRotation * GetScreenShake();
	}

	private Rotation GetScreenShake()
	{
		var nextTrauma = MathF.Max( 0f, Trauma - Time.Delta * TraumaDecayRate );
		Trauma = nextTrauma;
		var pitch = PitchShakeIntensity * Noise.Perlin( Time.Now * PitchShakeSpeed );
		var yaw = YawShakeIntensity * Noise.Perlin( ( Time.Now + 1 ) * YawShakeSpeed );
		var roll = RollShakeIntensity * Noise.Perlin( ( Time.Now + 1 ) * RollShakeSpeed );
		return new Angles( pitch, yaw, roll ) * Easing.EaseIn( Trauma ) * ScreenShakeScale;
	}
}
