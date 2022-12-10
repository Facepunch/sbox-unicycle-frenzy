
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

internal static class LeaderboardHelper
{

	//private static Global.Service.Leaderboard? Current;
	private static string CurrentName => GetLeaderboardName( Game.Server.MapIdent, TrailPass.CurrentSeason );

	public static async Task SubmitScore( IClient client, int score )
	{
		Game.AssertServer();
		/*
		if ( !await EnsureCurrentLeaderboard() )
		{
			Log.Error( "Failed to find or create leaderboard: " + CurrentName );
			return null;
		}

		if ( !Current.Value.CanSubmit )
		{
			Log.Error( $"Can't submit scores to {CurrentName}, why not?" );
			return null;
		}

		return await Current.Value.Submit( client, score );
		*/
	}

	public static async Task<List<int>> FetchScores()
	{
		var result = new List<int>();

		return result;
	}

	private static async Task<bool> EnsureCurrentLeaderboard()
	{
		return false;
	}

	private static string GetLeaderboardName( string mapIdent, int season, string bucket = "standard" )
	{
		return $"{mapIdent}.{season}.{bucket}";
	}

}
