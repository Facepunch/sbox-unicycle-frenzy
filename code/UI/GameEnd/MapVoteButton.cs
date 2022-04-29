using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class MapVoteButton : Button
{

	public Panel Thumbnail { get; set; }
	public string Votes { get; set; } = "0 votes";
	public string MapIdent { get; }

	public MapVoteButton( string mapIdent )
	{
		MapIdent = mapIdent;

		SetThumbnail();
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		UnicycleFrenzy.ServerCmd_SetMapVote( MapIdent );
	}

	private async void SetThumbnail()
	{
		var pkg = await Package.Fetch( MapIdent, true );
		if ( pkg == null )
		{
			Delete( false );
			return;
		}

		await Thumbnail.Style.SetBackgroundImageAsync( pkg.Thumb );
	}

	public override void Tick()
	{
		base.Tick();

		var voteCount = UnicycleFrenzy.Game.MapVotes.Values.Count( x => x == MapIdent );
		var localVoted = UnicycleFrenzy.Game.MapVotes.ContainsKey( Local.PlayerId ) && UnicycleFrenzy.Game.MapVotes[Local.PlayerId] == MapIdent;
		var term = voteCount == 1 ? "vote" : "votes";

		SetClass( "active", localVoted );
		Votes = $"{voteCount} {term}";
	}

}
