using Sandbox;
using System.Collections.Generic;
using System.Linq;

partial class UnicycleFrenzy : Sandbox.Game
{

	public static UnicycleFrenzy Game => Current as UnicycleFrenzy;

	[Net]
	public float GameTime { get; set; }
	[Net]
	public string NextMap { get; set; }

	private List<string> mapCycle = new()
	{
		"facepunch.uf_steps",
		"facepunch.uf_hop_stop",
		"facepunch.uf_pop_rock",
		"gvar.uf_canyon"
	};

	private List<string> fallMessages = new()
	{
		"{0} ate shit 💩",
		"{0} fell ass over tea-kettle",
		"Wow, did you see {0} bail that landing?",
		"{0} just went arse over tit!",
		"{0} adopted a tree this morning!",
		"{0} needs some practice 😂",
		"It's a skill problem for {0} 🤙",
		"{0} must have missed the \"wet floor\" warning"
	};

	public UnicycleFrenzy()
	{
		if ( IsClient )
		{
			new UnicycleHud();
		}

		if ( IsServer )
		{
			GameTime = 1800;
			NextMap = Rand.FromArray( mapCycle.Where( x => x != Global.MapName ).ToArray() );

			foreach( var part in UnicyclePart.All )
			{
				Precache.Add( part.Model );
			}
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );

		cl.Components.Add( new UnicycleEnsemble() );

		cl.Pawn = new UnicyclePlayer();
		(cl.Pawn as Player).Respawn();
	}

	public override void OnKilled( Client client, Entity pawn )
	{
		base.OnKilled( client, pawn );

		UfKillfeed.AddEntryOnClient( To.Everyone, GetRandomFallMessage( client.Name ), client.NetworkIdent );
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		if ( GameTime > 0 )
		{
			GameTime -= Time.Delta;

			if ( GameTime <= 0 )
			{
				Global.ChangeLevel( NextMap );
			}
		}
	}

	private int lastFallMessage;
	private string GetRandomFallMessage( string playerName )
	{
		var idx = Rand.Int( 0, fallMessages.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Rand.Int( 0, fallMessages.Count - 1 );

		lastFallMessage = idx;
		return string.Format( fallMessages[idx], playerName );
	}

}
