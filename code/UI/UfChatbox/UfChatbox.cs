
using Sandbox;
using Sandbox.UI;

[UseTemplate]
public partial class UfChatbox : Panel
{

	public static UfChatbox Current;

	public Panel EntryCanvas { get; set; }
	public TextEntry Input { get; set; }

	public bool IsOpen
	{
		get => HasClass( "open" );
		set
		{
			SetClass( "open", value );
			if ( value )
			{
				Input.Focus();
				Input.Text = string.Empty;
				Input.Label.SetCaretPosition( 0 );
			}
		}
	}

	public UfChatbox()
	{
		Current = this;

		Sandbox.Hooks.Chat.OnOpenChat += () =>
		{
			IsOpen = !IsOpen;
		};

		EntryCanvas.PreferScrollToBottom = true;
		EntryCanvas.TryScrollToBottom();

		Input.AddEventListener( "onsubmit", Submit );
		Input.AddEventListener( "onblur", () => IsOpen = false );
	}

	public override void OnHotloaded()
	{
		base.OnHotloaded();

		EntryCanvas.TryScrollToBottom();
	}

	public override void Tick()
	{
		base.Tick();
		
		SetClass( "topleft", GameEnd.IsOpen );

		if ( !IsOpen ) return;

		Input.Placeholder = string.IsNullOrEmpty( Input.Text ) ? "say something nice" : string.Empty;
	}

	public void AddEntry( string name, string message, string c = default, string sfx = default )
	{
		var entry = new UfChatboxEntry( name, message );
		if ( !string.IsNullOrEmpty( c ) ) entry.AddClass( c );
		EntryCanvas.AddChild( entry );

		if ( !string.IsNullOrEmpty( sfx ) )
		{
			Sound.FromScreen( sfx );
		}
	}

	private void Submit()
	{
		if ( string.IsNullOrWhiteSpace( Input.Text ) ) return;

		SendChat( Input.Text );
	}

	[ServerCmd]
	public static void SendChat( string message )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;

		AddChat( To.Everyone, ConsoleSystem.Caller.Name, message );
	}

	[ClientCmd( "uf_chat_add", CanBeCalledFromServer = true )]
	public static void AddChat( string name, string message, string classes = null, string sfx = "chat.message" )
	{
		Current?.AddEntry( name, message, classes, sfx );
	}

}

