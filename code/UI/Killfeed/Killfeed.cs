using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class Killfeed : Panel
{

	static Killfeed Current;

	public Killfeed()
	{
		Current = this;
	}

	[ConCmd.Client( "uf_killfeed_add", CanBeCalledFromServer = true )]
	public static void AddEntryOnClient( string message, int clientId )
	{
		Current?.AddEntry( message, true );
	}

}
