using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class CourseTimer : Panel
{

	public UnicycleFrenzy.GameStates GameState => UnicycleFrenzy.Game.GameState;

	public string CourseTime 
	{ 
		get
		{
			if ( Local.Pawn is not UnicyclePlayer pl ) return "UNKNOWN";

			var target = pl.SpectateTarget ?? pl;

			if ( target.TimerState != TimerState.Live )
				return FormattedTimeMs( 0 );

			return FormattedTimeMs( target.TimeSinceStart );
		} 
	}

	public string GameTime => FormattedTimeMs( UnicycleFrenzy.Game.StateTimer );

	public string NextMap => UnicycleFrenzy.Game.NextMap;

	public string MenuHotkey => Input.GetKeyWithBinding( "+iv_score" ) ?? "UNSET";
	public string MapKey => Input.GetKeyWithBinding( "+iv_score" ) ?? "UNSET";

	public static string FormattedTimeMsf( float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss\.ff" );
	}

	public static string FormattedTimeMs( float seconds )
	{
		return TimeSpan.FromSeconds( seconds ).ToString( @"m\:ss" );
	}

}

