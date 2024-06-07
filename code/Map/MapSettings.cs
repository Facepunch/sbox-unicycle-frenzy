
[Title( "Map Settings" )]
[Category( "Unicycle Frenzy" )]
[Icon( "settings" )]
[EditorHandle( "textures/editor/settings.png" )]
internal class MapSettings : Component
{
	[Property, Category( "Info" )] public string MapName { get; set; } = "Map Name";
	[Property, Category( "Info" )] public string Author { get; set; } = "Author";
	[Property, Category( "Info" )] public string Description { get; set; } = "Description";
	[Property, Category("Time")] public float BronzeTime { get; set; } = 60f;
	[Property, Category( "Time" )] public float SilverTime { get; set; } = 45f;
	[Property, Category( "Time" )] public float GoldTime { get; set; } = 30f;
	[Property, Category( "Time" )] public float PlatinumTime { get; set; } = 15f;
	[Property, Category( "Difficulty" )] public Difficulty Difficulty { get; set; } = Difficulty.Tier1;

	public List<FrenzyPickUp.FrenzyLetter> frenzyLetterList = new List<FrenzyPickUp.FrenzyLetter>();

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
	}
	public void AddTimePlayed( float seconds )
	{
		Local.TimePlayed += seconds;
		Save();
	}
	public void SetBestTime( float newTime )
	{
		if ( Local.BestTime != default && Local.BestTime < newTime ) return;
		Local.BestTime = newTime;
		Save();
	}

	public void MedalCheck( float time )
	{
		if ( time <= BronzeTime && !Local.HasBronzeMedal )
		{
			Local.HasBronzeMedal = true;
			AddXP( 2 );
		}
		if ( time <= SilverTime && !Local.HasSilverMedal )
		{
			Local.HasSilverMedal = true;
			AddXP( 4 );
		}
		if ( time <= GoldTime && !Local.HasGoldMedal )
		{
			Local.HasGoldMedal = true;
			AddXP( 6 );
		}
		if ( time <= PlatinumTime && !Local.HasPlatinumMedal )
		{
			Local.HasPlatinumMedal = true;
			AddXP( 8 );
		}
		Save();
	}

	public void AddXP( float xp )
	{
		var progression = DataHelper.ReadJson<UnicycleProgression>( "unicycle.progression.json" );
		progression.CurrentXP += xp;

		DataHelper.WriteJson( "unicycle.progression.json", progression );
	}

	public float GetBestTime()
	{
		Fetch();
		return Local.BestTime;
	}

	void Fetch()
	{
		Local = DataHelper.ReadJson<MapProgress>( "unicycle.stats." + MapName + ".json" );
		if(Local == null)
		{
			Local = new MapProgress();
			Local.CollectedFrenzy = false;
		}
	}

	private void Save()
	{
		if(Local == null)
		{
			Fetch();
		}
		DataHelper.WriteJson( "unicycle.stats." + MapName + ".json", Local );
	}

	public void OnFinish()
	{
		Local.Completions++;
		if ( frenzyLetterList.Count == 6 && !Local.CollectedFrenzy )
		{
			Local.CollectedFrenzy = true;
			AddXP( 10 );
		}
		Save();
	}

	public void FrenzyPickedUp( FrenzyPickUp.FrenzyLetter letter)
	{
		if(Local.CollectedFrenzy) return;

		if(frenzyLetterList.Contains(letter))
		{
			return;
		}

		frenzyLetterList.Add(letter);
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
	public bool HasBronzeMedal { get; set; }
	public bool HasSilverMedal { get; set; }
	public bool HasGoldMedal { get; set; }
	public bool HasPlatinumMedal { get; set; }
	public bool CollectedFrenzy { get; set; }
}

public enum Difficulty
{
	Tier1,
	Tier2,
	Tier3,
	Tier4
}
[GameResource( "Unicycle Frenzy Map", "ufmap", "UnicycleFrenzyMap", Icon = "adjust", IconBgColor = "#2e1236",IconFgColor = "#f0f0f0" )]
public class UnicycleFrenzyMap : GameResource
{
	[Property,Group("Info")] public string MapName { get; set; } = "Map Name";
	[Property,Group("Info")] public string Author { get; set; } = "Author";
	[Property,Group("Info"),TextArea] public string Description { get; set; } = "Description";
	[Property,Group( "Info" ),ImageAssetPath] public string MapIcon { get; set; } = "textures/ui/screenshot-1.jpg";
	[Property,Group("Scene")] public SceneFile MapScene { get; set; }
}

[GameResource( "Unicycle Frenzy Season", "ufsesn", "UnicycleFrenzySeason", Icon = "cloud", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UnicycleFrenzySeason : GameResource
{
	[Property,Group("Info")] public string SeasonName { get; set; } = "Season Name";
	[Property,Group("Info")] public string Author { get; set; } = "Author";
	[Property,Group("Info"),TextArea] public string Description { get; set; } = "Description";
	[Property,Group( "Info" ),ImageAssetPath] public string SeasonIcon { get; set; } = "textures/ui/screenshot-1.jpg";
	[Property,Group("Maps")] public List<UnicycleFrenzyMap> GreenMaps { get; set; } = new List<UnicycleFrenzyMap>();
	[Property,Group("Maps")] public List<UnicycleFrenzyMap> YellowMaps { get; set; } = new List<UnicycleFrenzyMap>();
	[Property,Group("Maps")] public List<UnicycleFrenzyMap> OrangeMaps { get; set; } = new List<UnicycleFrenzyMap>();
	[Property,Group("Maps")] public List<UnicycleFrenzyMap> RedMaps { get; set; } = new List<UnicycleFrenzyMap>();
}
