
using Sandbox.UI;

[UseTemplate]
internal class LeaderboardSignEntry : Panel
{

	public Label PlayerRank { get; set; }
	public Label PlayerName { get; set; }
	public Label CompletionTime { get; set; }

	public LeaderboardSignEntry( int rank, UnicyclePlayer pl )
	{
		PlayerRank.Text = rank.ToString();
		PlayerName.Text = pl.Client.Name;

		if ( pl.CourseIncomplete ) CompletionTime.Text = "INCOMPLETE";
		else CompletionTime.Text = pl.BestTime.FormattedTimeMsf();

		if ( pl.IsLocalPawn ) AddClass( "local" );
	}

}
