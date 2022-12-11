
using Sandbox;
using System.Collections.Generic;

partial class UnicycleFrenzy : GameManager
{

	public static UnicycleFrenzy Game => Current as UnicycleFrenzy;

	private List<string> fallMessages = new()
	{
		"{0} ate shit 💩",
		"{0} fell ass over tea-kettle",
		"Wow, did you see {0} bail that landing?",
		"{0} just went arse over tit!",
		"{0} adopted a tree this morning!",
		"{0} needs some practice 😂",
		"It's a skill problem for {0} 🤙",
		"{0} must have missed the \"wet floor\" warning",
		"{0} had an oopsy!",
		"That wasn't insane {0}",
		"{0} lost the plot!"
	};

	public UnicycleFrenzy()
	{
		if ( Sandbox.Game.IsClient )
		{
			new UnicycleRootPanel();
		}

		if ( Sandbox.Game.IsServer )
		{
			foreach( var part in ResourceLibrary.GetAll<CustomizationPart>() )
			{
				if( !string.IsNullOrEmpty( part.Model ) ) Precache.Add( part.Model );
				if( !string.IsNullOrEmpty( part.Particle ) ) Precache.Add( part.Particle );
				if( !string.IsNullOrEmpty( part.Texture ) ) Precache.Add( part.Texture );
			}

			InitMapCycle();
			_ = GameLoopAsync();
		}
	}

	public override void ClientJoined( IClient cl )
	{
		base.ClientJoined( cl );

		cl.Components.Add( new CustomizationComponent() );

		cl.Pawn = new UnicyclePlayer();
		(cl.Pawn as Player).Respawn();

		if ( cl.IsBot )
		{
			(cl.Pawn as UnicyclePlayer).BestTime = new System.Random().Next( 180, 1800 );
		}

		UfChat.AddChat( To.Everyone, "Server", $"{cl.Name} has joined the game", sfx: "player.joined" );

		NotifyPlayersNeeded();
	}

	public override async void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		UfChat.AddChat( To.Everyone, "Server", $"{cl.Name} has left the game", sfx: "player.left" );

		// this lets the client fully disconnect before we count and notify
		await Task.Delay( 1 );

		NotifyPlayersNeeded();
	}

	public override void OnKilled( IClient client, Entity pawn )
	{
		base.OnKilled( client, pawn );

		Killfeed.AddEntryOnClient( To.Everyone, GetRandomFallMessage( client.Name ), client.NetworkIdent );
	}

	private int lastFallMessage;
	private string GetRandomFallMessage( string playerName )
	{
		var idx = Sandbox.Game.Random.Int( 0, fallMessages.Count - 1 );
		while ( idx == lastFallMessage )
			idx = Sandbox.Game.Random.Int( 0, fallMessages.Count - 1 );

		lastFallMessage = idx;
		return string.Format( fallMessages[idx], playerName );
	}

}
