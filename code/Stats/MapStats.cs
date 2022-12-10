
using Sandbox;

internal class MapStats
{

	public int Falls { get; set; }
	public int Respawns { get; set; }
	public int Attempts { get; set; }
	public int Completions { get; set; }
	public float BestTime { get; set; }
	public float TimePlayed { get; set; }

	public void AddFall()
	{
		Falls++;
		Save();
	}

	public void AddRespawn()
	{
		Respawns++;
		Save();
	}

	public void AddAttempt()
	{
		Attempts++;
		Save();
	}

	public void AddCompletion()
	{
		if ( Completions == 0 )
		{
			Event.Run( "mapstats.firstcompletion" );
		}

		Completions++;
		Save();
	}

	public void SetBestTime( float newTime )
	{
		if ( BestTime != default && BestTime < newTime ) return;
		BestTime = newTime;
		Save();
	}

	public void AddTimePlayed( float seconds )
	{
		TimePlayed += seconds;
		Save();

		Event.Run( "mapstats.ontimeplayed", TimePlayed );
	}

	private void Save() => Cookie.Set( "unicycle.stats." + Game.Server.MapIdent, this );
	public static MapStats Local => Cookie.Get<MapStats>( "unicycle.stats." + Game.Server.MapIdent, new() );

}
