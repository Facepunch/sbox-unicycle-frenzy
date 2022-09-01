
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class CustomizeCategoryButton : Button
{


	private readonly PartType PartType;
	private static CustomizeCategoryButton activeBtn;

	public CustomizeCategoryButton( PartType type )
	{
		PartType = type;
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

		var parts = ResourceLibrary.GetAll<CustomizationPart>().Where( x => x.PartType == PartType );
		customizeTab.BuildParts( parts );
	}

}
