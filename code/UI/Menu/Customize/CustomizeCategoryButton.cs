
using Facepunch.Customization;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class CustomizeCategoryButton : Button
{

	public CustomizationCategory Category { get; }

	private static CustomizeCategoryButton activeBtn;

	public CustomizeCategoryButton( CustomizationCategory category )
	{
		Category = category;
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		SetActive();
	}

	public void SetActive()
	{
		activeBtn?.RemoveClass( "active" );
		AddClass( "active" );
		activeBtn = this;

		var customizeTab = Ancestors.OfType<CustomizeTab>().FirstOrDefault();
		if ( customizeTab == null ) return;

		var cfg = Customization.Config;
		customizeTab.BuildParts( cfg.Parts.Where( x => x.CategoryId == Category.Id ) );
	}

}
