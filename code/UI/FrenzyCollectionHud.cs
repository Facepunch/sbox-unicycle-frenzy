
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

internal class FrenzyCollectionHud : Panel
{

	public static FrenzyCollectionHud Current;

	private Dictionary<FrenzyCollectible.FrenzyLetter, Label> Letters = new();

	private TimeSince TimeSinceDisplayed;

	public FrenzyCollectionHud()
	{
		foreach ( FrenzyCollectible.FrenzyLetter val in Enum.GetValues( typeof( FrenzyCollectible.FrenzyLetter ) ) )
		{
			var label = Add.Label( val.ToString() );
			Letters[val] = label;
		}

		Current = this;
	}

	public override void Tick()
	{
		base.Tick();

		foreach( var kvp in Letters )
		{
			kvp.Value.SetClass( "collected", FrenzyCollectionHelper.Contains( kvp.Key ) );
		}

		if( TimeSinceDisplayed > 5f )
		{
			RemoveClass( "open" );
		}
	}

	public void Display()
	{
		TimeSinceDisplayed = 0f;
		AddClass( "open" );
	}

}
