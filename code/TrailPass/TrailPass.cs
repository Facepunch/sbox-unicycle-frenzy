﻿using System.Collections.Generic;

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
					new() { Id = 1, RequiredExperience = 10, DisplayName = "Item", PartId = 45 },
					new() { Id = 2, RequiredExperience = 40, DisplayName = "Item 2", PartId = 49 },
					new() { Id = 3, RequiredExperience = 60, DisplayName = "Item 3", PartId = 55 },
					new() { Id = 4, RequiredExperience = 100, DisplayName = "Item 4", PartId = 59 },
					new() { Id = 5, RequiredExperience = 120, DisplayName = "Item 5", PartId = 68 },
					new() { Id = 6, RequiredExperience = 140, DisplayName = "Item 6", PartId = 70 },
					new() { Id = 7, RequiredExperience = 1000, DisplayName = "Item 7", PartId = 72 }
				},
				Achievements = new()
				{
					new() { Id = 1, AchievementShortName = "uf_bronze", ExperienceGranted = 30 },
					new() { Id = 2, AchievementShortName = "uf_silver", ExperienceGranted = 40 },
					new() { Id = 3, AchievementShortName = "uf_gold", ExperienceGranted = 50 },
				}
			};
		}
	}
}
