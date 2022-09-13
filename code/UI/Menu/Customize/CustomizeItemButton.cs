using Sandbox;
using Sandbox.UI;
using System.IO;
using System.Linq;

[UseTemplate]
internal class CustomizeItemButton : Panel
{

	public Panel PartIcon { get; set; }
	public string Tag { get; set; }
	public CustomizationPart Part { get; }

	public CustomizeItemButton( CustomizationPart part )
	{
		Part = part;

		SetIcon();
		UpdateState();
		AddClass( "button-sound" ); 
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if( !Part.CanEquip() )
		{
			Toaster.Toast( "You haven't unlocked that yet", Toaster.ToastTypes.Error );
			return;
		}

		var customization = Local.Client.Components.Get<CustomizationComponent>();
		customization.Equip( Part );

		Ancestors.OfType<CustomizeTab>().FirstOrDefault()?.BuildRenderScene();
	}

	public override void Tick()
	{
		base.Tick();

		UpdateState();
	}

	private void UpdateState()
	{
		var canequip = Part.CanEquip();
		var customization = Local.Client.Components.Get<CustomizationComponent>();
		SetClass( "is-selected", customization.IsEquipped( Part ) );
		SetClass( "is-locked", !canequip );

		Tag = canequip ? string.Empty : 100.ToString();
	}

	private void SetIcon()
	{
		switch ( Part.PartType )
		{
			case PartType.Wheel:
			case PartType.Seat:
				new PartScenePanel( Part, true ).Parent = PartIcon;
				break;
			case PartType.Frame:
			case PartType.Pedal:
			case PartType.Trail:
				new PartScenePanel( Part, false ).Parent = PartIcon;
				break;
			case PartType.Spray:
				PartIcon.Style.SetBackgroundImage( Part.Texture );
				
				break;
			default:
				SetClass( "missing-icon", false );
				break;
		}
	}

}

