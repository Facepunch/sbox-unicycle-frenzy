
using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
[NavigatorTarget( "menu/achievements/view/{package_ident}" )]
internal class AchievementsView : Panel
{

	public Package Package { get; private set; }
	public ProgressBar ProgressBar { get; protected set; }
	public Panel Thumbnail { get; protected set; }
	public Panel Canvas { get; protected set; }

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Rebuild();
	}

	public override void SetProperty( string name, string value )
	{
		if( name != "package_ident" )
		{
			base.SetProperty( name, value );
			return;
		}

		if ( string.IsNullOrEmpty( value ) )
			return;

		LoadPackage( value );
		Rebuild();
	}

	private async void LoadPackage( string ident )
	{
		var pkg = await Package.Fetch( ident, true );
		if ( pkg == null ) return;

		Package = pkg;
	}

	private void Rebuild()
	{
		if ( Package == null ) 
			return;

		Canvas.DeleteChildren( true );

		var mapAchievements = Achievement.FetchForMap( Package.FullIdent ).Where( x => !x.PerMap );
		var completedCount = mapAchievements.Count( x => x.IsCompleted() );
		var totalCount = mapAchievements.Count();

		ProgressBar.Set( completedCount, totalCount );
		Thumbnail.Style.SetBackgroundImage( Package.Thumb );

		foreach( var ach in mapAchievements )
		{
			var entry = new AchievementsViewEntry( ach );
			entry.Parent = Canvas;
		}
	}

}
