﻿
using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal partial class Achievement
{

	//
	// todo: there's some code confusion with PerMap and MapName
	// there's probably a better way to separate the two, but we'll see 
	// what kinda headache it causes first
	//

	public long AchievementId { get; set; }
	public string GameName { get; set; }
	public string MapName { get; set; }
	public string ShortName { get; set; }
	public string DisplayName { get; set; }
	public string Description { get; set; }
	public string Thumbnail { get; set; }
	public bool PerMap { get; set; }

	public bool IsCompleted()
	{
		var map = PerMap ? Global.MapName : MapName;
		var playerid = Local.PlayerId;
		return AchievementCompletion.Query( playerid, GameName, map ).Any( x => x.AchievementId == AchievementId );
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="mapName">Leaving null will fetch for the current map</param>
	/// <returns></returns>
	public static IEnumerable<Achievement> FetchForMap( string mapName = null )
	{
		mapName ??= Global.MapName;

		// hack to make shit work if its ident.map, map, or local.map
		var map = mapName;
		if ( map.Contains( '.' ) )
			map = map.Split( '.' )[1];

		return All.Where( x => ( x.MapName != null && x.MapName.EndsWith( map ) ) || x.PerMap );
	}

	public static IEnumerable<Achievement> FetchGlobal()
	{
		return All.Where( x => x.MapName == null && !x.PerMap );
	}

	public static IEnumerable<Achievement> Fetch( string shortname = null, string map = null )
	{
		if ( !string.IsNullOrEmpty( map ) && map.Contains( '.' ) )
			map = map.Split( '.' )[1];

		var result = All.Where( x => x.GameName == Global.GameIdent );

		if ( !string.IsNullOrEmpty( map ) )
			result = result.Where( x => ( x.MapName != null && x.MapName.EndsWith( map ) ) || x.PerMap );

		if ( !string.IsNullOrEmpty( shortname ) ) 
			result = result.Where( x => x.ShortName == shortname );

		return result;
	}

	public static void Set( long playerid, string shortname, string map = null )
	{
		Host.AssertClient();

		var achievements = !string.IsNullOrEmpty( map )
			? FetchForMap( map )
			: FetchGlobal();

		var ach = achievements.FirstOrDefault( x => x.ShortName == shortname );

		if ( ach == null ) return;
		if ( ach.IsCompleted() ) return;

		var mapToInsert = ach.PerMap ? map : ach.MapName;

		AchievementCompletion.Insert( playerid, ach.AchievementId, mapToInsert );

		Event.Run( "achievement.set", shortname );
	}

	public static List<Achievement> All
	{
		get
		{
			var result = new List<Achievement>();

			result.Add( new Achievement()
			{
				AchievementId = 2,
				Description = "Complete any map in Unicycle Frenzy",
				DisplayName = "Unicyclist",
				ShortName = "uf_unicyclist",
				GameName = Global.GameIdent,
				Thumbnail = "https://files.facepunch.com/crayz/1b2711b1/unicyclist.png"
			} );

			result.Add( new Achievement()
			{
				AchievementId = 3,
				Description = "Complete the map in an ok amount of time",
				DisplayName = "Bronze Medal",
				ShortName = "uf_bronze",
				GameName = Global.GameIdent,
				Thumbnail = "https://files.facepunch.com/crayz/1b2711b1/medal_bronze.png",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 4,
				Description = "Complete the map in a decent amount of time",
				DisplayName = "Silver Medal",
				ShortName = "uf_silver",
				GameName = Global.GameIdent,
				Thumbnail = "https://files.facepunch.com/crayz/1b2711b1/medal_silver.png",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 5,
				Description = "Complete the map in a good amount of time",
				DisplayName = "Gold Medal",
				ShortName = "uf_gold",
				GameName = Global.GameIdent,
				Thumbnail = "https://files.facepunch.com/crayz/1b2711b1/medal_gold2.png",
				PerMap = true
			} );

			result.Add( new Achievement()
			{
				AchievementId = 6,
				Description = "An achievement specifically for uf_playground",
				DisplayName = "Playground",
				ShortName = "uf_playground_test",
				GameName = Global.GameIdent,
				MapName = "uf_playground",
				Thumbnail = ""
			} );

			result.Add( new Achievement()
			{
				AchievementId = 7,
				Description = "Complete the tutorial",
				DisplayName = "Tutorial",
				ShortName = "uf_complete_tutorial",
				GameName = Global.GameIdent,
				MapName = "uf_tutorial",
				Thumbnail = ""
			} );

			result.Add( new Achievement()
			{
				AchievementId = 12,
				Description = "Complete the map without falling",
				DisplayName = "Expert",
				ShortName = "uf_expert",
				GameName = Global.GameIdent,
				PerMap = true,
				Thumbnail = "https://files.facepunch.com/crayz/1b0911b1/juggler.png"
			} );

			result.Add( new Achievement()
			{
				AchievementId = 13,
				Description = "Collect all the letters of F-R-E-N-Z-Y",
				DisplayName = "FRENZY",
				ShortName = "uf_frenzy",
				GameName = Global.GameIdent,
				PerMap = true,
				Thumbnail = "https://files.facepunch.com/sbox/asset/facepunch.unicycle_frenzy/logo.c35e8d94.png"
			} );

			return result;
		}
	}

}
