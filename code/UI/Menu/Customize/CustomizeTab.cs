
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;
using System;

[UseTemplate]
[NavigatorTarget( "menu/customize" )]
internal class CustomizeTab : Panel
{

	public CustomizeRenderScene RenderScene { get; set; }
	public Panel CategoryTabs { get; set; }
	public Panel PartsList { get; set; }
	public CustomizationPart HoveredPart { get; set; }

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		BuildRenderScene();
		BuildPartCategories();
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		BuildRenderScene();
		BuildPartCategories();
	}

	public void BuildRenderScene()
	{
		RenderScene?.Build();
	}

	public void BuildParts( IEnumerable<CustomizationPart> parts )
	{
		PartsList.DeleteChildren( true );

		parts = parts.OrderBy( x => CanEquip( x ) ? 0 : 1 );

		foreach ( var part in parts )
		{
			if ( !part.CanEquip() )
			{
				continue;
			}

			var icon = new CustomizeItemButton( part );
			icon.Parent = PartsList;
			icon.AddEventListener( "onmouseover", x =>
			{
				HoveredPart = part;
			} );
		}
	}

	private void BuildPartCategories()
	{
		CategoryTabs.DeleteChildren( true );

		foreach( PartType partType in Enum.GetValues<PartType>() )
		{
			CategoryTabs.AddChild( new CustomizeCategoryButton( partType ) );
		}

		(CategoryTabs.Children.First() as CustomizeCategoryButton)?.SetActive();
	}

	private static bool CanEquip( CustomizationPart part )
	{
		if ( part.IsDefault )
			return true;
		if ( TrailPassProgress.Current.IsUnlocked( part ) )
			return true;

		return false;
	}

}
