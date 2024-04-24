using Sandbox;

public sealed class FrenzyPickUp : Component, Component.ITriggerListener
{
	[Property] public FrenzyLetter Letter { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		var Panel = Components.Get<FrenzyLetterWorldUI>( FindMode.InSelf );
		if ( Panel == null ) return;
		Panel.MyStringValue = Letter.ToString();

		var hasPickedUp = MapSettings.Local.CollectedFrenzy;
		if ( hasPickedUp )
		{
			Panel.UpdateState( "hidden", true );
		}
	}

	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ply = other.Components.Get<UnicycleController>();
		if ( ply == null ) return;
		var mapSettings = Scene.GetAllComponents<MapSettings>().FirstOrDefault();
		if ( mapSettings == null ) return;
		mapSettings.FrenzyPickedUp( Letter );
		var collider = Components.Get<SphereCollider>(FindMode.InSelf);
		collider.Enabled = false;

		var Panel = Components.Get<FrenzyLetterWorldUI>( FindMode.InSelf );
		Panel.UpdateState( "holding", true );
	}

	public void OnRestart()
	{
		var collider = Components.Get<SphereCollider>(FindMode.InSelf);
		collider.Enabled = true;

		var Panel = Components.Get<FrenzyLetterWorldUI>( FindMode.InSelf );
		if ( MapSettings.Local.CollectedFrenzy )
		{
			Panel.UpdateState( "hidden", true );
			return;
		}

		Panel.UpdateState( "holding", false );
	}

	public enum FrenzyLetter
	{
		F,
		R,
		E,
		N,
		Z,
		Y
	}

	public enum Result
	{
		None,
		Added,
		Completed
	}
}
