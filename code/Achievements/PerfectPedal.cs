﻿public class PerfectPedalBronze : BaseAchievement
{
	public override string AchievementName => "Perfect Pedal Bronze";
	public override string AchievementDescription => "Perfect Pedal 1000 times";
	public override string AchievementIcon => "textures/sprays/spray_facepunch.png";
	public override int AchievementPoints => 10;
	public override bool AchievementUnlocked { get; set; } = false;
	public override int NeededValue { get; set; } = 1000;
	public override float XPGiven { get; set; } = 10f;
}

public class PerfectPedalSilver : BaseAchievement
{
	public override string AchievementName => "Perfect Pedal Silver";
	public override string AchievementDescription => "Perfect Pedal 5000 times";
	public override string AchievementIcon => "textures/sprays/spray_facepunch.png";
	public override int AchievementPoints => 20;
	public override bool AchievementUnlocked { get; set; } = false;
	public override int NeededValue { get; set; } = 5000;
	public override float XPGiven { get; set; } = 20f;

}

public class PerfectPedalGold : BaseAchievement
{
	public override string AchievementName => "Perfect Pedal Gold";
	public override string AchievementDescription => "Perfect Pedal 10000 times";
	public override string AchievementIcon => "textures/sprays/spray_facepunch.png";
	public override int AchievementPoints => 30;
	public override bool AchievementUnlocked { get; set; } = false;
	public override int NeededValue { get; set; } = 10000;
	public override float XPGiven { get; set; } = 30f;

}

public class PerfectPedalPlatinum : BaseAchievement
{
	public override string AchievementName => "Perfect Pedal Platinum";
	public override string AchievementDescription => "Perfect Pedal 50000 times";
	public override string AchievementIcon => "textures/sprays/spray_facepunch.png";
	public override int AchievementPoints => 40;
	public override bool AchievementUnlocked { get; set; } = false;
	public override int NeededValue { get; set; } = 50000;
	public override float XPGiven { get; set; } = 50f;

}
