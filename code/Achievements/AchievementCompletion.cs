
using System;
using System.Collections.Generic;
using System.Linq;

internal class AchievementCompletion
{

	public string ShortName { get; set; } 
	public long SteamId { get; set; }
	public string MapName { get; set; }
	public DateTimeOffset Time { get; set; }

	public static List<AchievementCompletion> Query( long playerid, string map = null )
	{
		var result = new List<AchievementCompletion>();
		var all = All.Where( x => x.SteamId == playerid );

		if ( !string.IsNullOrEmpty( map ) )
		{
			all = all.Where( x => x.MapName != null && x.MapName.EndsWith( map ) );
		}

		var achievements = Achievement.All;

		foreach( var completion in all )
		{
			var ach = achievements.FirstOrDefault( x => x.ShortName == completion.ShortName );
			if ( ach == null ) continue;
			result.Add( completion );
		}

		return result;
	}

	public static void Insert( long playerid, string shortname, string map = null )
	{
		var final = new List<AchievementCompletion>( All )
		{
			new AchievementCompletion()
			{
				ShortName = shortname,
				SteamId = playerid,
				MapName = map,
				Time = DateTime.Now
			}
		};

		Cookie.Set( "uf.achievement_completions", final );
	}

	private const string AchievementCookie = "uf.achievement_completions";
	public static List<AchievementCompletion> All => Cookie.Get<List<AchievementCompletion>>( AchievementCookie, new () );

}
