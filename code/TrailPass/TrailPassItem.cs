
using Sandbox;

internal struct TrailPassItem
{
	public int Id { get; set; }
	public int RequiredExperience { get; set; }
	public string DisplayName { get; set; }
	[ResourceType( "upart" )]
	public CustomizationPart Part { get; set; }

}
