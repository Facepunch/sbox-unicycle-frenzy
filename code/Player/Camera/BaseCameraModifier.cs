
using Sandbox;
using System.Collections.Generic;

internal class BaseCameraModifier
{

	public static List<BaseCameraModifier> All = new();

	public BaseCameraModifier()
	{
		All.Add( this );
	}

	public static void PostCameraSetup( ref CameraSetup cam )
	{
		for ( int i = All.Count - 1; i >= 0; i-- )
		{
			if ( !All[i].Update( ref cam ) )
				All.RemoveAt( i );
		}
	}

	public virtual bool Update( ref CameraSetup cam )
	{
		return false;
	}

}
