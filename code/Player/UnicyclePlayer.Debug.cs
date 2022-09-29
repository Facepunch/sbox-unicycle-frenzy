using Sandbox;

internal partial class UnicyclePlayer
{

	[ConVar.Replicated( "uf_debug_playground_ramp" )]
	public static bool DebugRamp { get; set; } = false;

	[ConVar.Replicated( "uf_debug_playground_down_slope" )]
	public static bool DebugDownSlope { get; set; } = false;

	[Net, Predicted]
	public TimeSince TimeSinceDebug { get; set; }

	[Event.Tick]
	private void OnTick()
	{
		if ( DebugRamp && TimeSinceDebug > 1f )
		{
			TimeSinceDebug = 0f;
			Position = new Vector3( 3550.32f, 693.15f - 90.46f, -122f );
			Rotation = Rotation.From( -1.08f, 1.41f, -0.00f );
			Velocity = Rotation.Forward.WithZ( 0 ) * 1500f;
			return;
		}

		if( DebugDownSlope && TimeSinceDebug > 3f )
		{
			TimeSinceDebug = 0f;
			Position = new Vector3( 1946.94f, 1472.23f, 305.53f );
			Rotation = Rotation.From( 12.10f, -116.20f, 0.00f );
			Velocity = Rotation.Forward.WithZ( 0 ) * 200f;
			return;
		}
	}

}

