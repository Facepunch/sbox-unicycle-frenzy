
using System.Collections.Generic;

internal class TrailPassProgress
{

	public int TrailPassId { get; set; }
	public int Experience { get; set; }
	public List<string> UnlockedItems { get; set; } = new();

	public bool IsUnlocked( CustomizationPart part )
	{
		return UnlockedItems.Contains( part.ResourceName );
	}

	public void Unlock( CustomizationPart part ) 
	{ 
		if ( IsUnlocked( part ) ) return;
		UnlockedItems.Add( part.ResourceName );
	}

	public void Save()
	{
		Cookie.Set( SeasonCookie, this );
	}

	public static TrailPassProgress Current => Cookie.Get<TrailPassProgress>( SeasonCookie, new() );
	private static string SeasonCookie => "uf.trailpass." + TrailPass.CurrentSeason;

}
