
using Sandbox;

internal struct TrailPassItem
{

	public string DisplayName { get; set; }
	public int ExperienceNeeded { get; set; }
	[ResourceType( "upart" )]
	public string Part { get; set; }

	public CustomizationPart FindPart() => ResourceLibrary.Get<CustomizationPart>( Part );

}
