
using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class CourseTimer : Panel
{

	public Panel CheckpointHintCanvas { get; protected set; }

	public string CourseTime
	{
		get
		{
			if ( Local.Pawn is not UnicyclePlayer pl ) return "UNKNOWN";

			var target = pl.SpectateTarget ?? pl;

			if ( target.TimerState != TimerState.Live )
				return 0f.FormattedTimeMs();

			return target.TimeSinceStart.FormattedTimeMs();
		}
	}

	public string GameTime => ((float)UnicycleFrenzy.Game.StateTimer).FormattedTimeMs();
	public string MenuHotkey => InputActions.Menu.GetButtonOrigin();
	public string MapKey => InputActions.Scoreboard.GetButtonOrigin();

	[Event("unicycle.checkpoint.set")]
	public void ShowCheckpointHint( UnicyclePlayer pl )
	{
		if ( Local.Pawn != pl ) return;

		CheckpointHintCanvas.DeleteChildren( true );

		var hint = new CheckpointHint();
		hint.Text = "Checkpoint Reached";

		CheckpointHintCanvas.AddChild( hint );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "is-live", UnicycleFrenzy.Game.GameState == UnicycleFrenzy.GameStates.Live );
	}

	public static string FormattedTimeMsf( float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss\.ff" );
	}

	public class CheckpointHint : Label
	{

		public TimeSince TimeSinceCreated;

		public CheckpointHint()
		{
			TimeSinceCreated = 0;
		}

		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceCreated > 3f )
			{
				Delete();
			}
		}

	}

}

