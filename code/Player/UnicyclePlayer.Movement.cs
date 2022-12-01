using Sandbox;

internal partial class UnicyclePlayer
{

	// might have to rethink some of this, got a lot of moving parts..
	// maybe client authoritative movement would suit us better

	[Net, Predicted]
	public float PedalPosition { get; set; } // -1 = left pedal down, 1 = right pedal down, 0 = flat
	[Net, Predicted]
	public Angles Tilt { get; set; }
	[Net, Predicted]
	public float TimeToReachTarget { get; set; }
	[Net, Predicted]
	public float PedalStartPosition { get; set; }
	[Net, Predicted]
	public float PedalTargetPosition { get; set; }
	[Net, Predicted]
	public float TimeSincePedalStart { get; set; }
	[Net, Predicted]
	public Rotation TargetForward { get; set; }
	[Net, Predicted]
	public float TimeSinceJumpDown { get; set; }
	[Net, Predicted]
	public float TimeSinceNotGrounded { get; set; }
	[Net, Predicted]
	public Angles JumpTilt { get; set; }
	[Net, Predicted]
	public Angles PrevJumpTilt { get; set; }
	[Net, Predicted]
	public Vector3 PrevVelocity { get; set; }
	[Net, Predicted]
	public bool PrevGrounded { get; set; }
	[Net, Predicted]
	public bool Fallen { get; set; }
	[Net, Predicted]
	public float SurfaceFriction { get; set; }

	private bool overrideRot;
	private Rotation rotOverride;

	public void Fall( bool incrementFallStat = true )
	{
		if ( Fallen ) return;

		Sound.FromWorld( "unicycle.crash.default", Position );

		Game.Current.DoPlayerSuicide( Client );

		Event.Run( "unicycle.fall", this );

		Particles.Create( "particles/player/slamland.vpcf", Position );

		if( incrementFallStat ) AddFallOnClient();

		ResetMovement();
		Fallen = true;
		FallCount++;
	}

	public void ResetMovement()
	{
		PedalPosition = 0;
		Tilt = Angles.Zero;
		TimeToReachTarget = 0;
		PedalStartPosition = 0;
		PedalTargetPosition = 0;
		TimeSincePedalStart = 0;
		TargetForward = Rotation;
		JumpTilt = Angles.Zero;
		PrevJumpTilt = Angles.Zero;
		PrevVelocity = Vector3.Zero;
		PrevGrounded = false;
		TimeSinceNotGrounded = 0;
		TimeSinceJumpDown = 0;
		Fallen = false;
		SurfaceFriction = 1f;
	}

	public override void BuildInput()
	{
		base.BuildInput();

		if ( !overrideRot ) return;

		if ( Local.Pawn is Player pl )
			pl.ViewAngles = rotOverride.Angles();

		overrideRot = false;
	}

	[ClientRpc]
	private void SetRotationOnClient( Rotation rotation )
	{
		overrideRot = true;
		rotOverride = rotation;
	}

	[ClientRpc]
	private void AddFallOnClient()
	{
		if ( !IsLocalPawn ) return;
		MapStats.Local.AddFall();
	}

}

