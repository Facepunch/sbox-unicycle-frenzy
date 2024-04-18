
[Title( "Map Settings" )]
[Category( "Unicycle Frenzy" )]
[Icon( "settings" )]
[EditorHandle( "textures/editor/settings.png" )]
internal class MapSettings : Component
{
	[Property, Category("Time")] public float BronzeTime { get; set; } = 60f;
	[Property, Category( "Time" )] public float SilverTime { get; set; } = 45f;
	[Property, Category( "Time" )] public float GoldTime { get; set; } = 30f;
	[Property, Category( "Time" )] public float PlatinumTime { get; set; } = 15f;
	[Property, Category( "Difficulty" )] public Difficulty Difficulty { get; set; } = Difficulty.Tier1;

	protected override void OnStart()
	{
		Fetch();
	}

	protected override void OnUpdate()
	{

	}
	public void AddFall()
	{
		Local.Falls++;
		Save();

		Log.Info( "Falls: " + Local.Falls );
	}

	void Fetch()
	{
		Local = FileSystem.Data.ReadJson<MapProgress>( "unicycle.stats." + Game.ActiveScene.Title + ".json" );
		if(Local == null)
		{
			Local = new MapProgress();
		}
	}

	private void Save()
	{
		if(Local == null)
		{
			Fetch();
		}
		FileSystem.Data.WriteJson( "unicycle.stats." + Game.ActiveScene.Title + ".json", Local );
	}

	public static MapProgress Local;
}

public class MapProgress
{
	public int Falls { get; set; }
	public int Respawns { get; set; }
	public int Attempts { get; set; }
	public int Completions { get; set; }
	public float BestTime { get; set; }
	public float TimePlayed { get; set; }
}

public enum Difficulty
{
	Tier1,
	Tier2,
	Tier3,
	Tier4
}
