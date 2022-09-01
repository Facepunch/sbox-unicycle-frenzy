
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

		SetClass( "unlocked", progress.IsUnlocked( item.Part ) );
		SetClass( "unlockable", progress.Experience >= item.RequiredExperience );

		var part = Item.Part;
		var lookright = part.PartType == PartType.Wheel || part.PartType == PartType.Seat;
		partPanel = new PartScenePanel( part, lookright );
		partPanel.RotationSpeed = 25f;

		Thumbnail.AddChild( partPanel );
	}

	public override void Tick()
	{
		base.Tick();

		var progress = TrailPassProgress.Current;
		SetClass( "unlocked", progress.IsUnlocked( Item.Part ) );
		SetClass( "unlockable", Item.RequiredExperience <= progress.Experience );
	}

	public void TryUnlock()
	{
		var progress = TrailPassProgress.Current;

		if ( progress.IsUnlocked( Item.Part ) )
		{
			Toaster.Toast( "You already unlocked that", Toaster.ToastTypes.Simple );
			return;
		}

		if ( Item.RequiredExperience > progress.Experience )
		{
			Toaster.Toast( $"You need {Item.RequiredExperience} xp!", Toaster.ToastTypes.Error );
			return;
		}

		progress.Unlock( Item.Part );
		progress.Save();

		Toaster.Toast( $"Item unlocked!", Toaster.ToastTypes.Celebrate );
	}

	protected override void OnMouseOver( MousePanelEvent e )
	{
		base.OnMouseOver( e );
		
		partPanel.RenderOnce = false;
	}

	protected override void OnMouseOut( MousePanelEvent e )
	{
		base.OnMouseOut( e );

		partPanel.RenderOnce = true;
	}

}
