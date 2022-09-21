
using Sandbox;
using Sandbox.UI;
using System.Linq;

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

		var totalAchievements = Achievement.FetchForMap( Package.FullIdent ).Where( x => !x.PerMap );
		var completedCount = totalAchievements.Count( x => x.IsCompleted() );
		var totalCount = totalAchievements.Count();

		ProgressBar.Set( completedCount, totalCount );

		SetClass( "is-complete", totalCount > 0 && completedCount == totalCount );
	}

	protected override void OnMouseDown( MousePanelEvent e )
	{
		base.OnMouseDown( e );

		Ancestors.OfType<GameMenu>()?.FirstOrDefault()?.Navigate( $"menu/achievements/view/{Package.FullIdent}" );
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
