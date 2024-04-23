[GameResource( "Unicycle Frenzy Items", "ufitem", "UnicycleFrenzyItems", Icon = "category", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UnicycleFrenzyItems : GameResource
{
	public string ItemName { get; set; }
	public string ItemDescription { get; set; }
	[ImageAssetPath] public string ItemImage { get; set; } = "textures/ui/map-thumbnail-placeholder.png";
	public Model ItemModel { get; set; }
	public ItemCategories ItemCategory { get; set; } = ItemCategories.Frame;
	public ItemRare ItemRarity { get; set; } = ItemRare.Common;

	/// <summary>
	/// Icon for this clothing piece.
	/// </summary>
	[Hide]
	public IconSetup Icon { get; set; }

	public struct IconSetup
	{
		public string Path { get; set; }
	}
}

public class UnicycleDressed
{
	public UnicycleFrenzyItems Frame { get; set; }
	public UnicycleFrenzyItems Seat { get; set; }
	public UnicycleFrenzyItems Wheel { get; set; }
	public UnicycleFrenzyItems Accessory { get; set; }
	public UnicycleFrenzyItems Pedal { get; set; }
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
			Frame.Model = Local.Frame.ItemModel;
			Seat.Model = Local.Seat.ItemModel;
			Wheel.Model = Local.Wheel.ItemModel;
			//Accessory.Model = Local.Accessory.ItemModel;
			LeftPedal.Model = Local.Pedal.ItemModel;
			RightPedal.Model = Local.Pedal.ItemModel;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Local != null )
		{
			Local = FileSystem.Data.ReadJson<UnicycleDressed>( "unicycle.dress.json" );
			Frame.Model = Local.Frame.ItemModel;
			Seat.Model = Local.Seat.ItemModel;
			Wheel.Model = Local.Wheel.ItemModel;
			//Accessory.Model = Local.Accessory.ItemModel;
			LeftPedal.Model = Local.Pedal.ItemModel;
			RightPedal.Model = Local.Pedal.ItemModel;
		}

	}
}
