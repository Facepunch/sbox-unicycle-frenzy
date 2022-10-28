
public partial class UfChat : Panel
{

	private static UfChat Instance;

	public UfChat()
	{
		Instance = this;
	}
	
	[ConCmd.Server]
	public static void SendChat( string message )
	{
		if ( !ConsoleSystem.Caller.IsValid() ) return;
		
		AddChat( To.Everyone, ConsoleSystem.Caller.Name, message );
	}

	[ConCmd.Client( "uf_chat_add", CanBeCalledFromServer = true )]
	public static void AddChat( string name, string message, string classes = null, string sfx = "chat.message" )
	{
		Instance.AddMessage( name, message );
	}

}
