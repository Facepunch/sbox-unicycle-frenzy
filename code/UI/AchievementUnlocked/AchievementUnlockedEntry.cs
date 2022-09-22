
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class AchievementUnlockedEntry : Panel
{

	private TimeSince TimeSinceDisplayed;

	public Panel Icon { get; set; }
	public Label DisplayName { get; set; }
	public Label Experience { get; set; }

	public AchievementUnlockedEntry() { }
	public AchievementUnlockedEntry( Achievement achievement )
	{
		Icon.Style.SetBackgroundImage( achievement.Thumbnail );
		DisplayName.Text = achievement.DisplayName;
		Experience.Text = string.Empty;

		if ( TrailPass.Current.TryGetAchievement( achievement.ShortName, out var ach ) )
		{
			Experience.Text = $"+{ach.ExperienceGranted}xp";
			SetClass( "has-experience", true );
		}

		TimeSinceDisplayed = 0;

		Sound.FromScreen( "sounds/ui/achievement.unlocked.sound", .8f, 0f );
	}

	public override void Tick()
	{
		base.Tick();

		if ( TimeSinceDisplayed > 6f )
			Delete();
	}

}
