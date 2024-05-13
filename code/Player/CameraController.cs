
using Sandbox;
using System;
using static Sandbox.Citizen.CitizenAnimationHelper;

internal class CameraController : Component
{

	[Property]
	public UnicycleController Target { get; set; }

	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public float FieldOfView { get; set; }

	public static Angles ViewAngles;
	CameraComponent Camera;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		Camera ??= Components.Get<CameraComponent>();

		if ( Camera == null ) return;
		if ( Target == null ) return;		

		if ( Target.Dead )
		{
			var overheadPositionOffset = Vector3.Up * 100;
			var overheadPosition = Target.Position + overheadPositionOffset;
			var overheadRotation = Rotation.LookAt( Vector3.Down, Vector3.Forward );

			Position = Vector3.Lerp( Position, overheadPosition, Time.Delta * 0.5f );
			Rotation = Rotation.Slerp( Rotation, overheadRotation, Time.Delta * 0.5f ); 
			FieldOfView = FieldOfView.LerpTo( 90f, Time.Delta * 0.5f );

			Transform.Position = Position;
			Transform.Rotation = Rotation;
			Camera.FieldOfView = Screen.CreateVerticalFieldOfView( FieldOfView );

			return;
		}

		var dir = (Position - Target.Position).Normal;
		var pos = Target.Position + dir * 15f;
		Sound.Listener = new Transform( pos, Rotation );

		//ClearViewBlockers();
		//UpdateViewBlockers( pawn );

		if( Target.LockTurning || Scene.GetAllComponents<TutorialHints>().Count() > 0f )
		{
			Input.AnalogLook = default;
		}

		var diff = Rotation.Difference( Rotation.From( 0, ViewAngles.yaw, 0 ), Rotation.From( 0, Target.Rotation.Yaw(), 0 ) );
		if ( diff.Angle() > 170 )
		{
			Input.AnalogLook = default;
		}

		ViewAngles += Input.AnalogLook * .25f;
		ViewAngles.pitch = Math.Clamp( ViewAngles.pitch, -35f, 65f );

		var targetRot = Rotation.From( ViewAngles );
		var center = Target.Position + Vector3.Up * 80;
		var distance = 150.0f * Target.Transform.Scale.x;
		var targetPos = center + targetRot.Forward * -distance;

		var tr = Scene.Trace.Ray( center, targetPos )
			.Radius( 8 )
			.WithoutTags( "player" )
			.Run();

		var endpos = tr.EndPosition;

		//if ( tr.Entity is UfProp ufp && ufp.NoCameraCollide || tr.Entity is Checkpoint ufcp && ufcp.NoCameraCollide )
		//	endpos = targetPos;

		Position = endpos;
		Rotation = targetRot;

		var spd = Target.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 62f.LerpTo( 82f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );

		Transform.Position = Position;
		Transform.Rotation = Rotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( FieldOfView );

		if ( isShaking && timeSinceShakeStarted < shakeDuration )
		{
			ApplyCameraShake();
		}
		else
		{
			isShaking = false;
		}
	}

	private bool isShaking = false;
	private float shakeIntensity = 0.5f;
	private float shakeDuration = 0.5f;
	private TimeSince timeSinceShakeStarted;

	public void PunchShake( float intensity, float duration )
	{
		isShaking = true;
		shakeIntensity = intensity;
		shakeDuration = duration;
		timeSinceShakeStarted = 0;  // Reset the timer
	}
	private void ApplyCameraShake()
	{
		float shakeAmountX = Random.Shared.Float( -1, 1 ) * shakeIntensity;
		float shakeAmountY = Random.Shared.Float( -1, 1 ) * shakeIntensity;
		float shakeAmountZ = Random.Shared.Float( -1, 1 ) * shakeIntensity;
		Transform.Position += new Vector3( shakeAmountX, shakeAmountY, shakeAmountZ );

		FieldOfView = FieldOfView.LerpTo( FieldOfView * shakeIntensity * 2, Time.Delta * 0.5f );
	}
}
