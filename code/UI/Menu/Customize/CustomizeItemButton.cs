using Sandbox;
using Sandbox.UI;
using System.IO;
using System.Linq;

[UseTemplate]
internal class CustomizeItemButton : Panel
{

	public Panel PartIcon { get; set; }
	public Panel StateTarget { get; set; }
	public string Tag { get; set; }
	public CustomizationPart Part { get; }

	public CustomizeItemButton( CustomizationPart part )
	{
		Part = part;

		SetIcon();
		UpdateState();

		// there's a style somewhere making button layouts not adhere to columns
		// so I just added this to give it the hover/click sound effects and cursor
		// and made this class a normal panel, not a button
		AddClass( "button-sound" ); 
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if( !CanEquip() )
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
		var canequip = CanEquip();
		var customization = Local.Client.Components.Get<CustomizationComponent>();
		StateTarget.SetClass( "is-selected", customization.IsEquipped( Part ) );
		StateTarget.SetClass( "is-locked", !canequip );

		Tag = canequip ? string.Empty : 100.ToString();
	}

	private bool CanEquip()
	{
		if ( TrailPassProgress.Current.IsUnlocked( Part ) ) 
			return true;

		if ( Part.IsDefault )
			return true;

		return false;
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
				var texname = Path.GetFileNameWithoutExtension( Part.AssetPath );
				var texpath = $"textures/sprays/{texname}.png";
				var tex = Texture.Load( FileSystem.Mounted, texpath, false );
				if ( tex == null ) return;
				PartIcon.Style.SetBackgroundImage( tex );
				
				break;
			default:
				SetClass( "missing-icon", false );
				break;
		}
	}

}

