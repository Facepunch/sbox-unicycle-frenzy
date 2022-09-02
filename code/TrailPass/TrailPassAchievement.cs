
using Sandbox;

internal struct TrailPassAchievement
{

	[ResourceType( "achv" )]
	public string Achievement { get; set; }
	public int ExperienceGranted { get; set; }

	public Achievement FindAchievement() => ResourceLibrary.Get<Achievement>( Achievement );

}
