
using System.Collections.Generic;
using System.Text.Json;

internal class TrailPassProgress
{

	public int TrailPassId { get; set; }
	public int Experience { get; set; }
	public List<int> UnlockedItems { get; set; } = new();


	public static TrailPassProgress CurrentSeason => Deserialize( Cookie.Get( SeasonCookie, "{}" ) );
	public bool IsUnlocked( int id ) => UnlockedItems.Contains( id );
	public bool IsUnlockedByPartId( int partid )
	{
		if( TrailPass.Current.TryGetItem( partid, out var item ) )
		{
			return IsUnlocked( item.Id );
		}

		return false;
	}
	public void Unlock( int id ) 
	{ 
		if ( IsUnlocked( id ) ) return;
		UnlockedItems.Add( id );
	}
	public void Save() => Cookie.Set( SeasonCookie, Serialize( this ) );

	private static string SeasonCookie => "uf.trailpass." + TrailPass.CurrentSeason;

	private static TrailPassProgress Deserialize( string json )
	{
		try
		{
			return JsonSerializer.Deserialize<TrailPassProgress>( json );
		}
		catch( System.Exception e )
		{
			Log.Error( e.Message );
		}
		return new() { TrailPassId = TrailPass.CurrentSeason };
	}

	private static string Serialize( TrailPassProgress ticket )
	{
		return JsonSerializer.Serialize( ticket );
	}

}
