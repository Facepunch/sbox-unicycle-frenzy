
using Sandbox.UI;

[UseTemplate]
internal class NewPlayerOverlay : Panel
{

	public bool Open
	{
		get => HasClass( "open" );
		set => SetClass( "open", value );
	}

	public Panel Warning { get; set; }

	public NewPlayerOverlay()
	{
		Open = !Global.MapName.EndsWith( "uf_tutorial" ) && !Cookie.Get( "uf.hasplayed", false );

		Cookie.Set( "uf.hasplayed", true );
	}

	public void Close() => Open = false;

	public void LoadTutorial()
	{
		if( !Global.IsListenServer )
		{
			Warning.Style.Display = DisplayMode.Flex;
			return;
		}

		UnicycleFrenzy.ServerCmd_ChangeMap( "facepunch.uf_tutorial" );
	}

}
