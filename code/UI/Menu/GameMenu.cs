using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class GameMenu : NavigatorPanel
{

	public GameMenu()
	{
		Navigate( "menu/stats" );
	}

	public void Close()
	{
		SetClass( "open", false );
	}

	[Event.BuildInput]
	private void BuildInput( InputBuilder b )
	{
		if ( b.Pressed( InputActions.Menu ) )
		{
			SetClass( "open", !HasClass( "open" ) );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer pl ) return;

		SetClass( "is-spectating", pl.SpectateTarget != null );
	}

	public void StopSpectating()
	{
		UnicyclePlayer.ServerCmd_SetSpectateTarget( -1 );
		Close();
	}

}

