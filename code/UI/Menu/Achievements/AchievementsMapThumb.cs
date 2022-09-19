
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class AchievementsMapThumb : Panel
{

	private Package Package;

	public string MapName => FriendlyName();
	public Panel MapImage { get; protected set; }
	public ProgressBar ProgressBar { get; protected set; }

	public AchievementsMapThumb() { }

	public AchievementsMapThumb( Package package )
	{
		Package = package;

		Rebuild();
	}

	private void Rebuild()
	{
		if ( Package == null ) 
			return;

		MapImage.Style.SetBackgroundImage( Package.Thumb );
		ProgressBar.Set( 1, 4 );
	}

	private string FriendlyName()
	{
		if ( Package == null ) 
			return "Unknown";

		var result = Package.Title;
		if ( result.StartsWith( "unicycle frenzy", System.StringComparison.OrdinalIgnoreCase ) )
			result = result[15..].Trim();

		return result;
	}

}
