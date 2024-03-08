using Sandbox;

public sealed class MouseRotate : Component
{
	[Property] public float XSpeed { get; set; }
	[Property] public float YSpeed { get; set; }
	[Property] public bool InvertX { get; set; }
	[Property] public bool InvertY { get; set; }
	[Property] public bool ClampPitch { get; set; } = false;
	[Property, ShowIf( nameof( ClampPitch ), true )]
	public float PitchMin { get; set; } = -30f;
	[Property, ShowIf( nameof( ClampPitch ), true )]
	public float PitchMax { get; set; } = 30f;
	[Property] public bool ClampRoll { get; set; } = false;
	[Property, ShowIf( nameof( ClampRoll ), true )]
	public float RollMin { get; set; } = -30f;
	[Property, ShowIf( nameof( ClampRoll ), true )]
	public float RollMax { get; set; } = 30f;
	[Property] public CameraComponent Camera { get; set; }

	protected override void OnStart()
	{
		Camera ??= Scene.Camera;
	}

	protected override void OnFixedUpdate()
	{
		if ( Camera is null )
			return;

		var inputVec = new Vector2( Input.AnalogLook.yaw, Input.AnalogLook.pitch );
		var xInput = inputVec.x * XSpeed * Time.Delta * (InvertX ? -1 : 1);
		var yInput = inputVec.y * YSpeed * Time.Delta * (InvertY ? -1 : 1);
		var upRotation = Rotation.FromAxis( Camera.Transform.Rotation.Up, xInput );
		var rightRotation = Rotation.FromAxis( Camera.Transform.Rotation.Right, yInput );
		var rotation = Transform.World
			.RotateAround( Transform.Position, upRotation )
			.RotateAround( Transform.Position, rightRotation )
			.Rotation;
		if ( ClampPitch || ClampRoll )
		{
			rotation = ClampRotation( rotation );
		}
		Transform.Rotation = rotation;
	}

	private Rotation ClampRotation( Rotation rotation )
	{
		Angles angles = rotation;
		if ( ClampPitch )
		{
			angles.pitch = angles.pitch.Clamp( PitchMin, PitchMax );
		}
		if ( ClampRoll )
		{
			angles.roll = angles.roll.Clamp( RollMin, RollMax );
		}
		return angles;
	}
}
