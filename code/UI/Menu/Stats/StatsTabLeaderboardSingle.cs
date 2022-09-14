using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
[NavigatorTarget( "menu/stats/leaderboard/{scope}" )]
internal class StatsTabLeaderboardSingle : NavigatorPanel
{

	private enum LeaderboardScope
	{
		None,
		Session,
		Global,
		Friends
	}

	private LeaderboardScope scope = LeaderboardScope.None;

	public Panel Canvas { get; set; }

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if( string.Equals(name, "scope" ) )
		{
			scope = Enum.Parse<LeaderboardScope>( value, true );

			RebuildLeaderboard();
		}
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		RebuildLeaderboard();
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		RebuildLeaderboard();
	}

	public void Rebuild()
	{
		RebuildLeaderboard();
	}

	private int sessionhash;
	public override void Tick()
	{
		base.Tick();

		if ( scope != LeaderboardScope.Session ) return;

		var newhash = 0;
		foreach ( var pl in Entity.All.OfType<UnicyclePlayer>().Where( x => x.IsValid() ) )
		{
			newhash = HashCode.Combine( newhash, pl.BestTime );
		}

		if( newhash != sessionhash )
		{
			sessionhash = newhash;
			RebuildLeaderboard();
		}
	}

	private async void RebuildLeaderboard()
	{
		if ( scope == LeaderboardScope.None ) return;

		Canvas.DeleteChildren( true );

		switch ( scope )
		{
			case LeaderboardScope.Global:
			case LeaderboardScope.Friends:
				var scores = await LeaderboardHelper.FetchScores();

				Canvas.DeleteChildren( true );

				if ( scope == LeaderboardScope.Friends )
				{
					scores = scores
						.Where( x => x.PlayerId == Local.PlayerId || new Friend( x.PlayerId ).IsFriend )
						.ToList();
				}

				foreach ( var entry in scores )
				{
					var time = entry.Score / 1000f;
					var el = new StatsTabLeaderboardEntry( entry.GlobalRank, entry.Name, time, entry.PlayerId );
					el.Parent = Canvas;
				}
				break;
			case LeaderboardScope.Session:
				var players = Player.All.OfType<UnicyclePlayer>()
					.Where( x => x.IsValid() && x.Client.IsValid() )
					.OrderBy( x => x.BestTime );

				var srank = 0;
				foreach( var pl in players )
				{
					var time = pl.CourseIncomplete ? 0f : pl.BestTime;
					var el = new StatsTabLeaderboardEntry( ++srank, pl.Client.Name, time, pl.Client.PlayerId );
					el.Parent = Canvas;
				}
				break;
		}
	}

}

