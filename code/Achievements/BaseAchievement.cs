public class BaseAchievement
{
	public virtual string AchievementName => "Achievement Name";
	public virtual string AchievementDescription => "Achievement Description";
	public virtual string AchievementIcon => "textures/ui/map-thumbnail-placeholder.png";
	public virtual int AchievementPoints => 10;
	public virtual bool AchievementUnlocked { get; set; } = false;
	public virtual int NeededValue { get; set; } = 100;
	public virtual int CurrentValue { get; set; } = 0;

	public virtual void OnAchievementUnlocked()
	{
		AchievementUnlocked = true;
	}

	public virtual void OnAchievementProgress()
	{
		if( AchievementUnlocked )
		{
			return;
		}

		CurrentValue++;
		Log.Info( $"Achievement {AchievementName} Progress: {CurrentValue}/{NeededValue}" );
		if ( CurrentValue >= NeededValue )
		{
			OnAchievementUnlocked();
		}

	}
}
