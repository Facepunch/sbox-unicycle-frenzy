
internal class CheckPointZone : BaseZone
{
	protected override Color ZoneColor => Color.Green;
	public bool CurrentCheckpoint { get; set; }

	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );

		var timer = player.Components.Get<CourseTimer>();

		if ( CurrentCheckpoint ) return;
		CurrentCheckpoint = true;

		CourseTimer.Local.CurrentCheckpoint = this;
		CourseTimer.Local.CheckpointsReached++;
	}

	protected override void OnPlayerExit( UnicycleController player )
	{
		base.OnPlayerExit( player );

		var timer = player.Components.Get<CourseTimer>();
	}

}
