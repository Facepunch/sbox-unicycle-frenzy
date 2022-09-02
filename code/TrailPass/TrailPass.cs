using Sandbox;
using System.Collections.Generic;
using System.Linq;

[GameResource( "Trail Pass", "tpass", "A trailpass definition" )]
internal class TrailPass : GameResource
{

	public int Season { get; set; }
	public string DisplayName { get; set; }
	public int ExperiencePerLevel { get; set; } = 10;
	public int MaxExperience { get; set; } = 1000;
	public List<TrailPassItem> Items { get; set; } = new();
	public List<TrailPassAchievement> Achievements { get; set; } = new();

	public const int CurrentSeason = 1;

	public bool TryGetAchievement( string shortname, out TrailPassAchievement ach )
	{
		ach = default;

		if ( !Achievements.Any( x => x.FindAchievement()?.ShortName == shortname ) )
			return false;

		ach = Achievements.First( x => x.FindAchievement()?.ShortName == shortname );

		return true;
	}

	public static TrailPass Current => ResourceLibrary.GetAll<TrailPass>().FirstOrDefault( x => x.Season == CurrentSeason );

}
