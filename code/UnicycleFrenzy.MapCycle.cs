
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	[Net]
	public string NextMap { get; set; }
	[Net]
	public IDictionary<long, string> MapVotes { get; set; }
	[Net]
	public IList<string> MapOptions { get; set; }

	private async void InitMapCycle()
	{
		Host.AssertServer();

		NextMap = Global.MapName;

		var packages = await Package.FindAsync( "type:map game:facepunch.unicycle_frenzy", 16 );
		var maps = packages.Packages.Select( x => x.FullIdent ).ToList();

		var pkg = await Package.Fetch( Global.GameIdent, true );
		if ( pkg != null )
		{
			maps.AddRange( pkg.GetMeta<List<string>>( "MapList", new() ) );
		}

		MapOptions = maps.OrderBy( x => Rand.Int( 9999 ) )
			.Distinct()
			.Where( x => x != Global.MapName )
			.Take( 5 )
			.ToList();
		NextMap = Rand.FromList( MapOptions.ToList() );
	}

	private async void ChangeMapWithDelay( string mapident, float delay )
	{
		Host.AssertServer();

		delay *= 1000f;
		var timer = 0f;
		while( timer < delay )
		{
			await Task.Delay( 100 );

			timer += 100;

			if( timer % 1000 == 0 )
			{
				var timeleft = ( delay - timer ) / 1000f;
				UfChat.AddChat( To.Everyone, "Server", $"{timeleft} seconds remaining." );
			}
		}

		UfChat.AddChat( To.Everyone, "Server", $"Changing level to {mapident}" );

		await Task.Delay( 3000 );

		ServerCmd_ChangeMap( mapident );
	}

	[ConCmd.Server]
	public static void ServerCmd_ChangeMap( string mapident )
	{
		Global.ChangeLevel( mapident );
	}

	[ConCmd.Server]
	public static void ServerCmd_SetMapVote( string mapIdent )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) 
			return;

		if ( Game.MapVotes.TryGetValue( ConsoleSystem.Caller.PlayerId, out var vote ) && vote == mapIdent )
			return;

		Game.MapVotes[ConsoleSystem.Caller.PlayerId] = mapIdent;

		var votemap = new Dictionary<string, int>();
		Game.MapVotes.Values.ToList().ForEach( x => votemap.Add( x, 0 ) );
		foreach ( var kvp in Game.MapVotes )
		{
			votemap[kvp.Value]++;
		}
		Game.NextMap = votemap.OrderByDescending( x => x.Value ).First().Key;

		UfChat.AddChat( To.Everyone, "Server", string.Format( "{0} voted for {1}", ConsoleSystem.Caller.Name, mapIdent ) );

		if ( CanForceChange( mapIdent ) )
		{
			UfChat.AddChat( To.Everyone, "Server", "A new level has been voted for!" );
			Game.ChangeMapWithDelay( mapIdent, 5f );
		}
	}

	private static bool CanForceChange( string mapIdent )
	{
		if ( Game.GameState != GameStates.FreePlay ) return false;

		var half = MathF.Ceiling( Client.All.Count / 2f );
		if ( Game.MapVotes.Values.Count( x => x == mapIdent ) >= half ) return true;

		return false;
	}

}
