[GameResource( "Unicycle Frenzy Item Pass", "ufpass", "UnicycleFrenzyItemPass", Icon = "airplane_ticket", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UFItemProgression : GameResource
{
	//Add the default ones
	public List<ItemProgress> ItemsInPass { get; set; } = new List<ItemProgress> {
		new ItemProgress { Item = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/default/defaultframe.ufitem" ), XPNeeded = 0, ShowInPass = false, DefaultItem = true },
		new ItemProgress { Item = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/default/defaultseat.ufitem" ), XPNeeded = 0, ShowInPass = false, DefaultItem = true },
		new ItemProgress { Item = ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/default/defaultwheel.ufitem" ), XPNeeded = 0, ShowInPass = false , DefaultItem = true},
		new ItemProgress { Item =  ResourceLibrary.Get<UnicycleFrenzyItems>( "resources/items/default/defaultpedal.ufitem" ), XPNeeded = 0, ShowInPass = false, DefaultItem = true }
		};
	public float MaxSeasonXP { get; set; }
	public bool IsCurrentPass { get; set; }

	public struct ItemProgress
	{
		public UnicycleFrenzyItems Item { get; set; }
		public float XPNeeded { get; set; }
		[Hide]public bool IsUnlocked { get; set; }
		public bool ShowInPass { get; set; }
		public bool DefaultItem { get; set; }
	}
}

public class UnicycleProgression
{
	public float CurrentXP { get; set; } = 1000;
	public List<UnicycleFrenzyItems> UnlockedItems { get; set; } = new List<UnicycleFrenzyItems>();
}
