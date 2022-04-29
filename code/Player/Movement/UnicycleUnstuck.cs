using Sandbox;

internal class UnicycleUnstuck : Unstuck
{

	internal int StuckTries = 0;

	public UnicycleUnstuck( BasePlayerController controller )
		: base( controller )
	{

	}

	public override bool TestAndFix()
	{
		var mins = (Controller as UnicycleController).Mins;
		var maxs = (Controller as UnicycleController).Maxs;
		var result = Controller.TraceBBox( Controller.Position, Controller.Position, mins, maxs );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		if ( result.StartedSolid )
		{
			if ( BasePlayerController.Debug )
			{
				DebugOverlay.Text( Controller.Position, $"[stuck in {result.Entity}]", Color.Red );
				Box( result.Entity, Color.Red );
			}
		}

		//
		// Client can't jiggle its way out, needs to wait for
		// server correction to come
		//
		if ( Host.IsClient )
			return true;

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.Position + Vector3.Random.Normal * (((float)StuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Controller.Position + Vector3.Up * 5;
			}

			result = Controller.TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Text( Controller.Position, $"unstuck after {StuckTries} tries ({StuckTries * AttemptsPerTick} tests)", Color.Green, 5.0f );
					DebugOverlay.Line( pos, Controller.Position, Color.Green, 5.0f, false );
				}

				Controller.Position = pos;
				return false;
			}
			else
			{
				if ( BasePlayerController.Debug )
				{
					DebugOverlay.Line( pos, Controller.Position, Color.Yellow, 0.5f, false );
				}
			}
		}

		StuckTries++;

		return true;
	}

}

