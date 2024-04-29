using Sandbox;

public sealed class ItemManager : Component
{
	UnicycleProgression Progression { get; set; }
	UFItemProgression ProgressionResource { get; set; }
	protected override void OnStart()
	{
		base.OnStart();


		ProgressionResource = ResourceLibrary.GetAll<UFItemProgression>().Where( x => x.IsCurrentPass ).FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		Fetch();

		if ( ProgressionResource != null )
		{
			GetProgress();
		}
		//Items = Progression.UnlockedItems;
	}

	public void GetProgress()
	{
		foreach ( var item in ProgressionResource.ItemsInPass )
		{
			if( Progression.UnlockedItems.Contains( item.Item ) ) continue;
			if( Progression.CurrentXP < item.XPNeeded ) continue;
			Progression.UnlockedItems.Add( item.Item );

			Save();
		}
	}

	void Fetch()
	{
		Progression = FileSystem.Data.ReadJson<UnicycleProgression>( "unicycle.progression.json" );
		if ( Progression == null )
		{
			Progression = new UnicycleProgression();
		}
	}

	void Save()
	{
		if ( Progression == null )
		{
			Fetch();
		}
		FileSystem.Data.WriteJson( "unicycle.progression.json", Progression );
	}
}
