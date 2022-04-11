
using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class GameEnd : Panel
{

	public Panel MapCanvas { get; set; }
	public Panel PodiumCanvas { get; set; }
	public string TimeLeft => CourseTimer.FormattedTimeMs( UnicycleFrenzy.Game.StateTimer );

	private bool manuallyopened = false;

	[Event.Frame]
	private void OnFrame()
	{
		var open = UnicycleFrenzy.Game.GameState == UnicycleFrenzy.GameStates.End || manuallyopened;

		SetClass( "open", open );

		if ( !open ) return;

		EnsureMaps();
	}

	private int maphash = 0;
	private void EnsureMaps()
	{
		var newhash = 0;
		var options = UnicycleFrenzy.Game.MapOptions;

		foreach ( var map in options )
		{
			newhash = HashCode.Combine( newhash, map );
		}

		if ( newhash == maphash ) return;

		maphash = newhash;
		Refresh();
	}

	private void Refresh()
	{
		PodiumCanvas.DeleteChildren( true );
		PodiumCanvas.AddChild( new PodiumPanel( 2 ) );
		PodiumCanvas.AddChild( new PodiumPanel( 1 ) );
		PodiumCanvas.AddChild( new PodiumPanel( 3 ) );

		MapCanvas.DeleteChildren( true );
		foreach ( var map in UnicycleFrenzy.Game.MapOptions )
		{
			var btn = new MapVoteButton( map );
			MapCanvas.AddChild( btn );
		}
	}

	protected override void PostTemplateApplied() => Refresh();
	public override void OnHotloaded() => Refresh();

	[Event.BuildInput]
	private void OnBuildInput( InputBuilder b )
	{
		var btn = InputActionsExtensions.GetInputButton( InputActions.Scoreboard );
		if ( b.Pressed( btn ) )
		{
			manuallyopened = !manuallyopened;
		}
	}

}
