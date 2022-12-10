
using Sandbox;
using System.Linq;
using System.Threading.Tasks;

internal partial class UnicycleFrenzy
{

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;
	[Net]
	public GameStates GameState { get; set; } = GameStates.FreePlay;

	private bool ForceStart;

	private async Task GameLoopAsync()
	{
		while ( !CanStart() )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		UfChat.AddChat( To.Everyone, "Server", "The match will start in 10 seconds", "alert", "chat.alert" );

		GameState = GameStates.PreMatch;
		StateTimer = 5f;
		await WaitStateTimer();

		UfChat.AddChat( To.Everyone, "Server", "The match is live!", "alert", "chat.alert" );

		GameState = GameStates.Live;
		StateTimer = 15f * 60;
		FreshStart();
		await WaitStateTimer();

		GameState = GameStates.End;
		StateTimer = 60f;
		AwardExp();
		await WaitStateTimer();

		Sandbox.Game.ChangeLevel( NextMap );
	}

	private async Task WaitStateTimer()
	{
		while ( StateTimer > 0 )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		// extra second for fun
		await Task.DelayRealtimeSeconds( 1.0f );
	}

	private bool CanStart()
	{
		return ForceStart || Sandbox.Game.Clients.Count >= 3;
	}

	private void AwardExp()
	{
		var players = All.OfType<UnicyclePlayer>()
			.Where( x => x.IsValid() )
			.OrderBy( x => x.BestTime )
			.ToArray();

		if ( players.Length < 3 ) return;

		players[0]?.AddTrailPassExperience( 50 );
		players[1]?.AddTrailPassExperience( 25 );
		players[2]?.AddTrailPassExperience( 10 );
	}

	private void FreshStart()
	{
		foreach( var cl in Sandbox.Game.Clients )
		{
			if ( cl.Pawn is not UnicyclePlayer pl ) continue;
			pl.ResetMovement();
			pl.ResetTimer();
			pl.ResetBestTime();
			pl.GotoBestCheckpoint();
		}
	}

	private void NotifyPlayersNeeded()
	{
		if ( GameState != GameStates.FreePlay ) return;
		if ( CanStart() ) return;

		var needed = 3;
		var current = Sandbox.Game.Clients.Count;

		UfChat.AddChat( To.Everyone, "Server", $"{current} out of {needed} players needed to start", "alert", "chat.alert" );
	}

	[ConCmd.Admin]
	public static void SkipStage()
	{
		if ( Current is not UnicycleFrenzy uf ) return;

		if( uf.GameState == GameStates.FreePlay )
		{
			uf.ForceStart = true;
		}

		uf.StateTimer = 1;
	}

	public enum GameStates
	{
		FreePlay,
		PreMatch,
		Live,
		End
	}

}
