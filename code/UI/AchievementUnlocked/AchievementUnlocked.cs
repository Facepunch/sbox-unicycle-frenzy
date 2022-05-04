using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class AchievementUnlocked : Panel
{

	private TimeSince timeSinceDisplayed;

	public Panel Icon { get; set; }
	public Label DisplayName { get; set; }
	public Label Experience { get; set; }

	public void Display( Achievement achievement )
	{
		Icon.Style.SetBackgroundImage( achievement.Thumbnail );
		DisplayName.Text = achievement.DisplayName;
		Experience.Text = string.Empty;

		var bpAch = TrailPass.Current.Achievements.FirstOrDefault( x => x.AchievementShortName == achievement.ShortName );
		if( bpAch != null )
		{
			Experience.Text = $"+{bpAch.ExperienceGranted}xp";
		}

		SetClass( "open", true );
		timeSinceDisplayed = 0;
	}

	public override void Tick()
	{
		base.Tick();

		if ( HasClass( "open" ) && timeSinceDisplayed > 6f )
			RemoveClass( "open" );
	}

	[Event("achievement.set")]
	public void OnAchievementSet( string shortname )
	{
		var ach = Achievement.All.FirstOrDefault( x => x.ShortName == shortname );
		if ( ach == null ) return;

		Display( ach );
	}

}
