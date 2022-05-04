
using Sandbox.UI;

[UseTemplate]
internal class StatsAchievementsEntry : Panel
{

	public Achievement Achievement { get; set; }
	public Panel Icon { get; set; }

	public StatsAchievementsEntry( Achievement achievement )
	{
		Achievement = achievement;
		Icon.Style.SetBackgroundImage( achievement.Thumbnail );
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "completed", Achievement.IsCompleted() );
	}

}
