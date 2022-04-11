
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

		GameState = GameStates.PreMatch;
		StateTimer = 10f;
		await WaitStateTimer();

		GameState = GameStates.Live;
		StateTimer = 15f * 60;
		FreshStart();
		await WaitStateTimer();

		GameState = GameStates.End;
		StateTimer = 60f;
		AwardExp();
		await WaitStateTimer();

		Global.ChangeLevel( NextMap );
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
		return ForceStart || Client.All.Count >= 3;
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
		foreach( var cl in Client.All )
		{
			if ( cl.Pawn is not UnicyclePlayer pl ) continue;
			pl.ResetMovement();
			pl.ResetTimer();
			pl.ResetBestTime();
			pl.GotoBestCheckpoint();
		}
	}

	[AdminCmd]
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
