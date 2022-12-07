
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

[StyleSheet("UI/Styles/frenzyletter.scss")]
internal class FrenzyLetter : WorldPanel
{

	public FrenzyCollectible Collectible { get; }

	private Vector3 PositionOffset;
	private bool Owning;

	public FrenzyLetter( FrenzyCollectible collectible )
	{
		Collectible = collectible;

		var width = 5000;
		var height = 5000;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );
		PositionOffset = Vector3.Random.WithZ( 0 ).Normal;

		Owning = FrenzyCollectionHelper.Contains( Collectible.Letter );

		Add.Label( collectible.Letter.ToString() );
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		var position = Collectible.Position + Vector3.Up * 32;
		var holding = Collectible.Holders.Contains( Local.Pawn as UnicyclePlayer );
		var owning = Owning || Collectible.Owners.Contains( Local.Pawn as UnicyclePlayer );

		if ( holding )
		{
			position = Local.Pawn.Position + PositionOffset * 32f + Vector3.Up * 24f;
		}
		
		var offs = (float)Math.Sin( Time.Now );
		Position = position + Vector3.Up * offs * 8f;
		Rotation = Rotation.LookAt( Camera.Position - Position );

		SetClass( "holding", holding );
		SetClass( "owning", owning );
		SetClass( "hidden", Collectible.IsHidden() );
	}

}
