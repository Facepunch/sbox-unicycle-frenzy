
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

internal static class LeaderboardHelper
{

	private static Leaderboard? Current;
	private static string CurrentName => GetLeaderboardName( Global.MapName, TrailPass.CurrentSeason );

	public static async Task<LeaderboardUpdate?> SubmitScore( Client client, int score )
	{
		Host.AssertServer();

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
	}

	public static async Task<List<LeaderboardEntry>> FetchScores()
	{
		var result = new List<LeaderboardEntry>();

		if( !await EnsureCurrentLeaderboard() )
		{
			Log.Error( "Failed to find or create leaderboard: " + CurrentName );
			return result;
		}

		var query = await Current.Value.GetGlobalScores( 100 );
		if( query != null )
		{
			result.AddRange( query );
		}

		return result;
	}

	private static async Task<bool> EnsureCurrentLeaderboard()
	{
		Current ??= await Leaderboard.FindOrCreate( CurrentName, true );

		if ( Current == null )
			return false;

		return true;
	}

	private static string GetLeaderboardName( string mapIdent, int season, string bucket = "standard" )
	{
		return $"{mapIdent}.{season}.{bucket}";
	}

}
