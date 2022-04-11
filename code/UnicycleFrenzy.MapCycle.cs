
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

internal partial class UnicycleFrenzy
{

	[Net]
	public string NextMap { get; set; }
	[Net]
	public Dictionary<long, string> MapVotes { get; set; }
	[Net]
	public List<string> MapOptions { get; set; }

	private async void InitMapCycle()
	{
		Host.AssertServer();

		NextMap = Global.MapName;

		var pkg = await Package.Fetch( Global.GameIdent, true );
		if ( pkg == null )
		{
			Log.Error( "Failed to load map cycle" );
			return;
		}

		var maps = pkg.GetMeta<List<string>>( "MapList" )
			.Where( x => x != Global.MapName )
			.OrderBy( x => Rand.Int( 99999 ) )
			.Take( 5 )
			.ToList();

		MapOptions = maps;
		NextMap = Rand.FromList( maps );
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
				UfChatbox.AddChat( To.Everyone, "Server", $"{timeleft} seconds remaining." );
			}
		}

		UfChatbox.AddChat( To.Everyone, "Server", $"Changing level to {mapident}" );

		await Task.Delay( 3000 );

		ServerCmd_ChangeMap( mapident );
	}

	[ServerCmd]
	public static void ServerCmd_ChangeMap( string mapident )
	{
		Global.ChangeLevel( mapident );
	}

	[ServerCmd]
	public static void ServerCmd_SetMapVote( string mapIdent )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) 
			return;

		if ( Game.MapVotes.TryGetValue( ConsoleSystem.Caller.PlayerId, out var vote ) && vote == mapIdent )
			return;

		Game.MapVotes[ConsoleSystem.Caller.PlayerId] = mapIdent;
		Game.NextMap = Game.MapVotes.OrderByDescending( x => x.Value ).First().Value;

		UfChatbox.AddChat( To.Everyone, "Server", string.Format( "{0} voted for {1}", ConsoleSystem.Caller.Name, mapIdent ) );

		if ( CanForceChange( mapIdent ) )
		{
			UfChatbox.AddChat( To.Everyone, "Server", "A new level has been voted for!" );
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
