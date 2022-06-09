
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class StatsAchievementsEntry : Panel
{

	public Achievement Achievement { get; set; }
	public Panel Icon { get; set; }
	public int Experience { get; set; }

	public StatsAchievementsEntry( Achievement achievement )
	{
		Achievement = achievement;
		Icon.Style.SetBackgroundImage( achievement.Thumbnail );


		var tpa = TrailPass.Current.Achievements.FirstOrDefault( x => x.AchievementShortName == Achievement.ShortName );
		if ( tpa == null || tpa.ExperienceGranted == 0 )
		{
			SetClass( "grantsxp", false );
			return;
		}

		SetClass( "grantsxp", true );
		Experience = tpa.ExperienceGranted;
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "completed", Achievement.IsCompleted() );
	}

}
