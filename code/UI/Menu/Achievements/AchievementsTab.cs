using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
[NavigatorTarget("menu/achievements")]
internal class AchievementsTab : NavigatorPanel
{

	public Panel Canvas { get; set; }

    public AchievementsTab()
    {
		//Navigate( "menu/stats/details" );

		Rebuild();
    }

	[Event.Hotload]
	private async void Rebuild()
	{
		Canvas.DeleteChildren();

		var query = new Package.Query
		{
			Type = Package.Type.Map,
			Order = Package.Order.User,
			Take = 16,
		};

		query.Tags.Add( "game:facepunch.unicycle_frenzy" );

		var packages = await query.RunAsync( default );
		var maps = packages?.ToList() ?? new();

		foreach( var map in maps )
		{
			Canvas.AddChild( new AchievementsMapThumb( map ) );
		}
	}

}
