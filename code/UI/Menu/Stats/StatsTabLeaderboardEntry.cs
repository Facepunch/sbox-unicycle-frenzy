using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class StatsTabLeaderboardEntry : Panel
{

	public int Rank { get; set; }
	public string Name { get; set; }
	public string Time { get; set; }

	private long playerid;

	public StatsTabLeaderboardEntry( int rank, string name, float score, long playerId = 0 )
	{
		Rank = rank;
		Name = name;
		Time = score == 0f ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( score );
		playerid = playerId;

		SetClass( "me", Local.PlayerId == playerId );
		SetClass( "friend", new Friend( playerId ).IsFriend );
		SetClass( "can-spectate", Client.All.Any( x => x.PlayerId == playerId ) );
	}

	public void SpectateThisPlayer()
	{
		var cl = Client.All.FirstOrDefault( x => x.PlayerId == playerid );
		if ( cl == null ) return;
		if ( !cl.Pawn.IsValid() ) return;

		UnicyclePlayer.ServerCmd_SetSpectateTarget( cl.Pawn.NetworkIdent );

		Ancestors.OfType<GameMenu>().FirstOrDefault()?.Close();
	}

}
