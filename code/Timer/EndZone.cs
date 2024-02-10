
internal class EndZone : BaseZone
{

	protected override Color ZoneColor => Color.Red;

	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );

		var timer = player.Components.Get<CourseTimer>();
		if ( timer == null ) return;
		if ( timer.State != TimerStates.Started ) return;

		timer.FinishTimer();
	}

}
