using System.Collections.Generic;

internal class TrailPass
{
	public int Id { get; set; }
	public string DisplayName { get; set; }
	public int ExperiencePerLevel { get; set; } = 10;
	public int MaxExperience { get; set; } = 1000;
	public List<TrailPassItem> Items { get; set; } = new();
	public List<TrailPassAchievement> Achievements { get; set; } = new();

	public const int Season = 1;

	public static TrailPass Current
	{
		get
		{
			return new()
			{
				Id = 1,
				DisplayName = "Test TrailPass",
				Items = new()
				{
					new() { Id = 1, RequiredExperience = 30, DisplayName = "Item", PartId = 95 },
					new() { Id = 2, RequiredExperience = 100, DisplayName = "Item 2", PartId = 93 },
					new() { Id = 3, RequiredExperience = 175, DisplayName = "Item 3", PartId = 96 },
					new() { Id = 4, RequiredExperience = 300, DisplayName = "Item 4", PartId = 91 },
					new() { Id = 5, RequiredExperience = 425, DisplayName = "Item 5", PartId = 62 },
					new() { Id = 6, RequiredExperience = 550, DisplayName = "Item 6", PartId = 65 },
					new() { Id = 7, RequiredExperience = 675, DisplayName = "Item 7", PartId = 94 },
					new() { Id = 8, RequiredExperience = 800, DisplayName = "Item 8", PartId = 72 },
					new() { Id = 9, RequiredExperience = 900, DisplayName = "Item 9", PartId = 87 },
					new() { Id = 10, RequiredExperience = 1000, DisplayName = "Item 10", PartId = 97 },
				},
				Achievements = new()
				{
					new() { Id = 1, AchievementShortName = "uf_bronze", ExperienceGranted = 30 },
					new() { Id = 2, AchievementShortName = "uf_silver", ExperienceGranted = 40 },
					new() { Id = 3, AchievementShortName = "uf_gold", ExperienceGranted = 50 },
					new() { Id = 4, AchievementShortName = "uf_expert", ExperienceGranted = 50 },
				}
			};
		}
	}
}
