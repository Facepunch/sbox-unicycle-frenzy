
using Sandbox.UI;

[UseTemplate]
internal class TrailPassItemIcon : Panel
{

	public TrailPassItem Item { get; set; }
	public Panel Thumbnail { get; set; }

	private PartScenePanel partPanel;

	public TrailPassItemIcon( TrailPassItem item )
	{
		this.Item = item;

		var progress = TrailPassProgress.Current;

		SetClass( "unlocked", progress.IsUnlocked( item.FindPart() ) );
		SetClass( "unlockable", progress.Experience >= item.ExperienceNeeded );

		var part = Item.FindPart();
		var lookright = part.PartType == PartType.Wheel || part.PartType == PartType.Seat;
		partPanel = new PartScenePanel( part, lookright );
		partPanel.RotationSpeed = 25f;

		Thumbnail.AddChild( partPanel );
	}

	public override void Tick()
	{
		base.Tick();

		var progress = TrailPassProgress.Current;
		SetClass( "unlocked", progress.IsUnlocked( Item.FindPart() ) );
		SetClass( "unlockable", Item.ExperienceNeeded <= progress.Experience );
	}

	public void TryUnlock()
	{
		var progress = TrailPassProgress.Current;

		if ( progress.IsUnlocked( Item.FindPart() ) )
		{
			Toaster.Toast( "You already unlocked that", Toaster.ToastTypes.Simple );
			return;
		}

		if ( Item.ExperienceNeeded > progress.Experience )
		{
			Toaster.Toast( $"You need {Item.ExperienceNeeded} xp!", Toaster.ToastTypes.Error );
			return;
		}

		progress.Unlock( Item.FindPart() );
		progress.Save();

		Toaster.Toast( $"Item unlocked!", Toaster.ToastTypes.Celebrate );
	}

}
