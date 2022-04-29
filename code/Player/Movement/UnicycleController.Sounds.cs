using Sandbox;
using System;

internal partial class UnicycleController
{

	public override void OnEvent( string name )
	{
		base.OnEvent( name );

		switch ( name )
		{
			case "land":
				var snd = Sound.FromWorld( "unicycle.land.default", Position );
				var vol = Math.Clamp( Math.Abs( pl.PrevVelocity.z ) / 600f, .15f, 1f );
				snd.SetVolume( vol );
				break;
			case "pedal":
				Sound.FromWorld( "unicycle.pedal", Position );
				break;
		}
	}

}
