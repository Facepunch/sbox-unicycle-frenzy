
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class AchievementsViewEntry : Panel
{

	public Achievement Achievement { get; }
	public Panel Thumbnail { get; protected set; }

	public AchievementsViewEntry() { }
	public AchievementsViewEntry( Achievement achievement ) 
	{
		Achievement = achievement;

		Rebuild();
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Rebuild();
	}

	[Event.Hotload]
	private void Rebuild() 
	{
		if ( Achievement == null ) 
			return;

		SetClass( "is-completed", Achievement.IsCompleted() );
		Thumbnail.Style.SetBackgroundImage( Achievement.Thumbnail );
	}

}
