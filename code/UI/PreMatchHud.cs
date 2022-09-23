
using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class PreMatchHud : Panel
{

	public int TimeLeft { get; set; } = 20;

	private TimeSince TimeSincePulse;

	[Event.Frame]
	private void OnFrame()
	{
		base.Tick();

		if ( UnicycleFrenzy.Game.GameState != UnicycleFrenzy.GameStates.PreMatch )
		{
			SetClass( "open", false );
			return;
		}

		SetClass( "open", true );

		var timeleft = MathX.CeilToInt( UnicycleFrenzy.Game.StateTimer );
		if( timeleft < TimeLeft )
		{
			TimeLeft = timeleft;
			Pulse();
		}

		if ( TimeSincePulse > .25f )
		{
			RemoveClass( "pulse" );
		}
	}

	private void Pulse()
	{
		TimeSincePulse = 0;
		SetClass( "pulse", true );

		if( TimeLeft >= 0 )
			Sound.FromScreen( "sounds/ui/timer.prematch.pulse.sound" );
	}

}
