
using Sandbox;
using System.Collections.Generic;
using System.Linq;

[GameResource( "Achievement", "achv", "An achievement definition" )]
internal partial class Achievement : GameResource
{

	public string ShortName { get; set; }
	public string DisplayName { get; set; }
	public string Description { get; set; }
	[ResourceType( "png" )]
	public string Thumbnail { get; set; }
	public string MapName { get; set; }
	public bool PerMap { get; set; }

	public bool IsCompleted()
	{
		var map = PerMap ? Global.MapName : MapName;
		var playerid = Local.PlayerId;
		return AchievementCompletion.Query( playerid, map ).Any( x => x.ShortName == ShortName );
	}

	public static IEnumerable<Achievement> FetchForMap( string mapName = null )
	{
		mapName ??= Global.MapName;

		return All.Where( x => ( x.MapName != null && x.MapName.EndsWith( mapName ) ) || x.PerMap );
	}

	public static IEnumerable<Achievement> FetchGlobal()
	{
		return All.Where( x => x.MapName == null && !x.PerMap );
	}

	public static IEnumerable<Achievement> Fetch( string shortname = null, string map = null )
	{
		IEnumerable<Achievement> result = All.ToList();

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

		AchievementCompletion.Insert( playerid, ach.ShortName, mapToInsert );

		Event.Run( "achievement.set", shortname );
	}

	public static IEnumerable<Achievement> All => ResourceLibrary.GetAll<Achievement>();

}
