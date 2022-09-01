
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

		int xp = 0;
		if ( TrailPass.Current.TryGetAchievement( Achievement.ShortName, out var ach ) )
		{
			xp = ach.ExperienceGranted;
		}

		SetClass( "grantsxp", xp > 0 );
		Experience = xp;
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "completed", Achievement.IsCompleted() );
	}

}
