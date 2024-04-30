public class BaseAchievement
{
	public virtual string AchievementName { get; set; } = "Achievement Name";
	public virtual string AchievementDescription { get; set; } = "Achievement Description";
	public virtual string AchievementIcon { get; set; } = "textures/ui/map-thumbnail-placeholder.png";
	public virtual int AchievementPoints { get; set; } = 10;
	public virtual bool AchievementUnlocked { get; set; } = false;
	public virtual int NeededValue { get; set; } = 10;
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
		if ( CurrentValue <= NeededValue )
		{
			OnAchievementUnlocked();
		}
	}
}
