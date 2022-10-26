
using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;

[UseTemplate]
internal class PodiumPanel : Panel
{

	public Panel RenderCanvas { get; set; }
	public int Rank { get; }
	public string Name { get; private set; }
	public string Time { get; private set; }

	public PodiumPanel( int rank )
	{
		Rank = rank;

		AddClass( rank.ToString() );
	}

	private int activehash;
	public override void Tick()
	{
		base.Tick();

		var players = Player.All.Where( x => x is UnicyclePlayer && x.IsValid() && x.Client.IsValid() ).ToList();
		var orderedPlayers = players.OrderBy( x => (x as UnicyclePlayer).BestTime );

		var player = orderedPlayers.Skip( Rank - 1 ).FirstOrDefault() as UnicyclePlayer;
		if ( player == null ) return;

		var newhash = HashCode.Combine( player.BestTime, player.SessionRank, player.NetworkIdent );
		if ( newhash == activehash ) return;

		activehash = newhash;

		UpdateData( player );
	}

	private void UpdateData( UnicyclePlayer pl )
	{
		RenderCanvas.DeleteChildren( true );

		var renderscene = new PodiumRenderScene( pl );
		var scale = Rank switch
		{
			1 => 1.25f,
			2 => 1f,
			3 => .75f,
			_ => 1f
		};
		renderscene.Style.Set( $"transform: scale({scale});" );
		RenderCanvas.AddChild( renderscene );

		Name = pl.Client.Name;
		Time = pl.BestTime.FormattedTimeMsf();
	}

}
