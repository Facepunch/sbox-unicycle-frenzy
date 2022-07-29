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
	public TimeSince TimeSinceNotGrounded { get; set; }
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

		Fallen = true;
		FallCount++;

		Sound.FromWorld( "unicycle.crash.default", Position );

		Game.Current.DoPlayerSuicide( Client );

		Event.Run( "unicycle.fall", this );

		if( incrementFallStat ) AddFallOnClient();
	}

	public void ResetMovement()
	{
		PedalPosition = default;
		Tilt = default;
		TimeToReachTarget = default;
		PedalStartPosition = default;
		PedalTargetPosition = default;
		TimeSincePedalStart = default;
		TargetForward = Rotation;
		JumpTilt = default;
		PrevJumpTilt = default;
		PrevVelocity = default;
		PrevGrounded = default;
		TimeSinceNotGrounded = default;
		TimeSinceJumpDown = default;
		Fallen = false;
	}

	public override void BuildInput( InputBuilder input )
	{
		base.BuildInput( input );

		if ( !overrideRot ) return;
		input.ViewAngles = rotOverride.Angles();
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

