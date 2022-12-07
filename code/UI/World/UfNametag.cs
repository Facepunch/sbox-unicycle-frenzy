using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

[StyleSheet("UI/Styles/ufnametag.scss")]
internal class UfNametag : WorldPanel
{

	private UnicyclePlayer player;
	private Label Label;

	public UfNametag( UnicyclePlayer player )
	{
		this.player = player;

		if ( player.IsLocalPawn ) AddClass( "local" );

		var width = 1000;
		var height = 1000;
		PanelBounds = new Rect( -width * .5f, -height * .5f, width, height );

		Label = Add.Label( string.Empty, "name" );
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		if ( !player.IsValid() ) return;
		if ( !player.Client.IsValid() ) return;
		if ( !player.Citizen.IsValid() ) return;

		var hat = player.Citizen.GetAttachment( "hat" ) ?? new Transform( player.EyePosition );
		var crowned = player.SessionRank == 1;
		var height = crowned ? 16 : 8;
		Position = hat.Position + Vector3.Up * height;
		Rotation = Rotation.LookAt( -Screen.GetDirection( new Vector2( Screen.Width * 0.5f, Screen.Height * 0.5f ) ) );
		Style.Opacity = player.IsLocalPawn ? 0 : player.GetRenderAlpha();

		if ( Style.Opacity <= 0 ) return;

		var rank = player.SessionRank;
		var name = player.Client.Name;

		Label.Text = player.CourseIncomplete ? name : $"#{rank} {name}";
	}

}
