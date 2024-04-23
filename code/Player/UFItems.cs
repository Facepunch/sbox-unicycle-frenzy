[GameResource( "Unicycle Frenzy Items", "ufitem", "UnicycleFrenzyItems", Icon = "category", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UnicycleFrenzyItems : GameResource
{
	public string ItemName { get; set; }
	public string ItemDescription { get; set; }
	[ImageAssetPath]public string ItemImage { get; set; }
	public Model ItemModel { get; set; }
	public ItemCategories ItemCategory { get; set; } = ItemCategories.Frame;
	public ItemRare ItemRarity { get; set; } = ItemRare.Common;
}

public enum ItemCategories
{
	Frame,
	Wheel,
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
