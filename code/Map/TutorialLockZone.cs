internal class TutorialLockZone : BaseZone
{
	public bool IsTutorial { get; set; } = true;

	[Property] public bool LockTurning { get; set; }
	[Property] public bool LockPedaling { get; set; }
	[Property] public bool LockBraking { get; set; }
	[Property] public bool LockJumping { get; set; }
	[Property] public bool LockLean { get; set; }
	[Property] public bool LockTilt { get; set; }


	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );

		if ( !IsTutorial ) return;

		player.LockBraking = LockBraking;
		player.LockJumping = LockJumping;
		player.LockLean = LockLean;
		player.LockPedaling = LockPedaling;
		player.LockTilt = LockTilt;
		player.LockTurning = LockTurning;

	}
}
