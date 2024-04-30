
public class AchievementManager
{
	public List<BaseAchievement> Achievements { get; set; } = new List<BaseAchievement>();
	public static AchievementManager Instance { get; set; }

	public void Fetch()
	{
		Instance = FileSystem.Data.ReadJson<AchievementManager>( "unicycle.achievements.json" );
		if ( Instance == null )
		{
			Instance = new AchievementManager();
			Instance.LoadAchievements();

			Save();
		}
	}

	public void Save()
	{
		if( Instance == null )
		{
			Fetch();
		}
		FileSystem.Data.WriteJson( "unicycle.achievements.json", Instance );
	}

	public BaseAchievement GetAchievement( string name )
	{
		return Achievements.Find( x => x.AchievementName == name );
	}

	public void LoadAchievements()
	{
		// Get all achievements
		Achievements = new List<BaseAchievement>
		{
			new PerfectPedalBronze(),
			new PerfectPedalSilver(),
			new PerfectPedalGold(),
			new PerfectPedalPlatinum()
		};
	}
}

