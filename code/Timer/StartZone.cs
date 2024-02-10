
internal class StartZone : BaseZone
{

	protected override Color ZoneColor => Color.Green;

	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );

		var timer = player.Components.Get<CourseTimer>();
		timer?.ResetTimer();
	}

	protected override void OnPlayerExit( UnicycleController player )
	{
		base.OnPlayerExit( player );

		var timer = player.Components.Get<CourseTimer>();
		timer?.StartTimer();
	}

}
