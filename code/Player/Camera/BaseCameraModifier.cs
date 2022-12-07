
using Sandbox;
using System.Collections.Generic;

internal class BaseCameraModifier
{
	public static List<BaseCameraModifier> All = new();

	public BaseCameraModifier()
	{
		All.Add( this );
	}

	public static void PostCameraSetup()
	{
		for ( int i = All.Count - 1; i >= 0; i-- )
		{
			if ( !All[i].Update() )
				All.RemoveAt( i );
		}
	}

	public virtual bool Update()
	{
		return false;
	}

}
