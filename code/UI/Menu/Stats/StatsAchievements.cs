using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class StatsAchievements : NavigatorPanel
{

	public Panel AchievementProgressBar { get; set; }
	public Panel AchievementCanvas { get; set; }
	public string AchievementCount { get; set; }

	private void RebuildAchievements()
	{
		// setting dummy achievements to debug/preview
		Achievement.Set( Local.PlayerId, "uf_dummy_complete" );
		Achievement.Set( Local.PlayerId, "uf_dummy_climb_complete", "willow.uf_climb" );

		AchievementCanvas.DeleteChildren( true );

		var mapAchievements = Achievement.FetchForMap().OrderByDescending( x => x.IsCompleted() );
		var total = 0;
		var achieved = 0;

		foreach ( var ach in mapAchievements )
		{
			if ( IsMedal( ach ) )
			{
				// let's hide medals if the map hasn't defined time thresholds
				if ( !Entity.All.Any( x => x is AchievementMedals ) )
					continue;

				// hard-coding medal times into the description
				ach.Description = GetMedalDescription( ach );
			}

			var entry = new StatsAchievementsEntry( ach );
			entry.Parent = AchievementCanvas;

			if ( ach.IsCompleted() )
			{
				achieved++;
			}

			total++;
		}

		AchievementCount = $"{achieved}/{total} Earned";
		AchievementProgressBar.Style.Width = Length.Percent( ((float)achieved / total) * 100f );
	}

	private static bool IsMedal( Achievement ach )
	{
		return new string[]
		{
			"uf_bronze",
			"uf_silver",
			"uf_gold"
		}.Contains( ach.ShortName );
	}

	private static int ExperienceGranted( Achievement ach )
	{
		var pass = TrailPass.Current;
		var tpAchi = pass.Achievements.FirstOrDefault( x => x.AchievementShortName == ach.ShortName );
		if ( tpAchi == null ) return 0;

		return tpAchi.ExperienceGranted;
	}

	private static string GetMedalDescription( Achievement ach )
	{
		var achMedals = Entity.All.FirstOrDefault( x => x is AchievementMedals ) as AchievementMedals;
		if ( !achMedals.IsValid() ) return ach.Description;

		var time = ach.ShortName switch
		{
			"uf_bronze" => achMedals.Bronze,
			"uf_silver" => achMedals.Silver,
			"uf_gold" => achMedals.Gold,
			_ => 0
		};

		return $"Complete the map in {CourseTimer.FormattedTimeMs( time )}s or better";
	}

	[Event.Entity.PostSpawn]
	private void PostEntitiesSpawned() => RebuildAchievements();
	public override void OnHotloaded() => RebuildAchievements();
	protected override void PostTemplateApplied() => RebuildAchievements();

}

