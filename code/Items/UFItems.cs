[GameResource( "Unicycle Frenzy Items", "ufitem", "UnicycleFrenzyItems", Icon = "category", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UnicycleFrenzyItems : GameResource
{
	public string ItemName { get; set; }
	public string ItemDescription { get; set; }
	[ImageAssetPath] public string ItemImage { get; set; } = "textures/ui/map-thumbnail-placeholder.png";
	public bool IsMaterial { get; set; } = true;
	public Model ItemModel { get; set; } = Model.Load( "models/unicycles/default/parts/frames/default_frame.vmdl" );
	public Material ItemMaterial { get; set; }
	public ItemCategories ItemCategory { get; set; } = ItemCategories.Frame;
	public ItemRare ItemRarity { get; set; } = ItemRare.Common;
	public List<Skin> Skins { get; set; }

	/// <summary>
	/// Icon for this clothing piece.
	/// </summary>
	[Hide]
	public IconSetup Icon { get; set; }

	public struct IconSetup
	{
		public string Path { get; set; }
	}

	public struct Skin
	{
		public Material Material { get; set; }
		public Color Color { get; set; }
	}
}

public class UnicycleDressed
{
	public UnicycleFrenzyItems Frame { get; set; }
	public int FrameSkin { get; set; }
	public UnicycleFrenzyItems Seat { get; set; }
	public int SeatSkin { get; set; }
	public UnicycleFrenzyItems Wheel { get; set; }
	public int WheelSkin { get; set; }
	public UnicycleFrenzyItems Accessory { get; set; }
	public int AccessorySkin { get; set; }
	public UnicycleFrenzyItems Pedal { get; set; }
	public int PedalSkin { get; set; }
}

public enum ItemCategories
{
	Frame,
	Wheel,
	Seat,
	Pedal,
	Accessory
}

public enum ItemRare
{
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

[Title( "Unicycle Dresser" )]
[Category( "Unicycle Frenzy" )]
[Icon( "shopping_cart" )]
internal class UnicycleDresser : Component
{
	[Property] ModelRenderer Frame { get; set; }	
	[Property] ModelRenderer Seat { get; set; }	
	[Property] ModelRenderer Wheel { get; set; }
	//[Property] ModelRenderer Accessory { get; set; }
	[Property] ModelRenderer LeftPedal { get; set; }
	[Property] ModelRenderer RightPedal { get; set; }

	public static UnicycleDressed Local;

	[Property] bool IsMenu { get; set; } = false;

	protected override void OnAwake()
	{
		base.OnAwake();

		Local = FileSystem.Data.ReadJson<UnicycleDressed>( "unicycle.dress.json" );

	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( Local != null )
		{
			SetUpUnicycle();
		}
		else
		{
			Local = new UnicycleDressed
			{
				Frame = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/defaultframe.ufitem" ),
				FrameSkin = 1,
				Seat = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/defaultseat.ufitem" ),
				SeatSkin = 99,
				Wheel = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/defaultwheel.ufitem" ),
				WheelSkin = 1,
				Pedal = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/defaultpedal.ufitem" ),
				PedalSkin = 99,
			};

			FileSystem.Data.WriteJson( "unicycle.dress.json", Local );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if(!IsMenu) return;

		if ( Local != null )
		{
			SetUpUnicycle();
		}

	}

	void SetUpUnicycle()
	{
		Local = FileSystem.Data.ReadJson<UnicycleDressed>( "unicycle.dress.json" );

		Frame.Model = Local.Frame.ItemModel;
		if ( Local.FrameSkin != 99 )
		{
			Frame.MaterialOverride = Local.Frame.Skins[Local.FrameSkin - 1].Material;
		}
		else
		{
			if(Local.Frame.IsMaterial && Local.Frame.ItemMaterial != null)
			{
				Frame.MaterialOverride = Local.Frame.ItemMaterial;
			}
			else
			{
				Frame.MaterialOverride = null;
			}
		}

		Wheel.Model = Local.Wheel.ItemModel;
		if ( Local.WheelSkin != 99 )
		{
			Wheel.MaterialOverride = Local.Wheel.Skins[Local.WheelSkin - 1].Material;
		}
		else
		{
			if(Local.Wheel.IsMaterial && Local.Wheel.ItemMaterial != null)
			{
				Wheel.MaterialOverride = Local.Wheel.ItemMaterial;
			}
			else
			{
				Wheel.MaterialOverride = null;
			}
			
		}

		LeftPedal.Model = Local.Pedal.ItemModel;
		if ( Local.PedalSkin != 99 )
		{
			LeftPedal.MaterialOverride = Local.Pedal.Skins[Local.PedalSkin - 1].Material;
		}
		else
		{
			if(Local.Pedal.IsMaterial && Local.Pedal.ItemMaterial != null)
			{
				LeftPedal.MaterialOverride = Local.Pedal.ItemMaterial;
			}
			else
			{
				LeftPedal.MaterialOverride = null;
			}
		}

		RightPedal.Model = Local.Pedal.ItemModel;
		if ( Local.PedalSkin != 99 )
		{
			RightPedal.MaterialOverride = Local.Pedal.Skins[Local.PedalSkin - 1].Material;
		}
		else
		{
			if(Local.Pedal.IsMaterial && Local.Pedal.ItemMaterial != null)
			{
				RightPedal.MaterialOverride = Local.Pedal.ItemMaterial;
			}
			else
			{
				RightPedal.MaterialOverride = null;
			}
		}

		Seat.Model = Local.Seat.ItemModel;
		if ( Local.SeatSkin != 99 )
		{
			Seat.MaterialOverride = Local.Seat.Skins[Local.SeatSkin - 1].Material;
		}
		else
		{
			if(Local.Seat.IsMaterial && Local.Seat.ItemMaterial != null)
			{
				Seat.MaterialOverride = Local.Seat.ItemMaterial;
			}
			else
			{
				Seat.MaterialOverride = null;
			}
		}
	}
}
