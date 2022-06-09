
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class StatsMapInfo : NavigatorPanel
{
	public MapStats Stats => MapStats.Local;
	public string BestTime => Stats.BestTime == 0 ? "INCOMPLETE" : CourseTimer.FormattedTimeMsf( Stats.BestTime );
	public string TimePlayed => CourseTimer.FormattedTimeMs( Stats.TimePlayed );
	public string MapName => Global.MapName;
	public Panel Thumbnail { get; set; }

	public StatsMapInfo()
	{
		SetThumbnail();
	}

	private async void SetThumbnail()
	{
		// todo: in-game screenshot for map thumb?
		var pgk = await Package.Fetch( Global.MapName, true );
		if ( pgk == null ) return;
		Thumbnail.Style.SetBackgroundImage( pgk.Thumb );
	}

	public override void Tick() => SetClass( "incomplete", Stats.BestTime == 0 );

}

