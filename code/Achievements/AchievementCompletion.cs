using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

internal class AchievementCompletion
{

	public long AchievementId { get; set; } // key
	public long SteamId { get; set; } // key
	// public long MapAssetId { get; set; } // key
	public string MapName { get; set; }
	public DateTimeOffset Time { get; set; }

	public static List<AchievementCompletion> Query( long playerid, string game, string map = null )
	{
		// hack to make shit work if its ident.map, map, or local.map
		if ( map != null && map.Contains( '.' ) )
			map = map.Split( '.' )[1];

		// later: fetch from api
		var result = new List<AchievementCompletion>();
		var all = All.Where( x => x.SteamId == playerid );

		if ( !string.IsNullOrEmpty( map ) )
		{
			all = all.Where( x => x.MapName != null && x.MapName.EndsWith( map ) );
		}

		var achievements = Achievement.All;

		foreach( var completion in all )
		{
			var ach = achievements.FirstOrDefault( x => x.AchievementId == completion.AchievementId );
			if ( ach == null ) continue;
			if ( ach.GameName != game ) continue;
			result.Add( completion );
		}

		return result;
	}

	public static void Insert( long playerid, long achievementid, string map = null )
	{
		var final = new List<AchievementCompletion>( All )
		{
			new AchievementCompletion()
			{
				AchievementId = achievementid,
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
