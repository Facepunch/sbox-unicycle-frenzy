
public class AchievementManager
{
	public List<BaseAchievement> Achievements { get; set; } = new List<BaseAchievement>();
	public static AchievementManager Instance { get; set; }

	public void Fetch()
	{
		Instance = DataHelper.ReadJson<AchievementManager>( "unicycle.achievements.json" );
		if ( Instance == null )
		{
			Instance = new AchievementManager();
			Instance.LoadAchievements(); // Load all predefined achievements if none exist
		}
		else
		{
			Instance.MergeAchievements(); // Ensure all predefined achievements are present
		}
		Save(); // Save the updated list back to storage
	}

	public void MergeAchievements()
	{
		var predefinedAchievements = LoadAchievements();
		foreach ( var achievement in predefinedAchievements )
		{
			// Check if the loaded instance already contains this achievement
			if ( !Instance.Achievements.Any( a => a.AchievementName == achievement.AchievementName ) )
			{
				Instance.Achievements.Add( achievement );
			}
		}
	}

	public void Save()
	{
		if ( Instance != null )
		{
			DataHelper.WriteJson( "unicycle.achievements.json", Instance );
		}
	}

	public BaseAchievement GetAchievement( string name )
	{
		return Achievements.Find( x => x.AchievementName == name );
	}

	public int GetProgress( string name )
	{
		var achievement = GetAchievement( name );
		if ( achievement == null )
			return 0;

		return achievement.CurrentValue;
	}

	public List<BaseAchievement> LoadAchievements()
	{
		// Return a list of all predefined achievements
		return new List<BaseAchievement>
		{
			new PerfectPedalBronze(),
			new PerfectPedalSilver(),
			new PerfectPedalGold(),
			new PerfectPedalPlatinum()
		};
	}

	public void UpdateAchievementProgress( string achievementName )
	{
		var achievement = AchievementManager.Instance.GetAchievement( achievementName );
		if ( achievement != null && !achievement.AchievementUnlocked )
		{
			achievement.OnAchievementProgress();
			AchievementManager.Instance.Save();  // Consider moving Save outside if performance is a concern
		}
	}
}

