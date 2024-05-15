using Sandbox.VR;

internal class UnicycleUnstuck : Component
{
	internal int StuckTries = 0;
	public UnicycleController Controller { get; set; }

	public bool TestAndFix()
	{
		var mins = (Controller as UnicycleController).Mins;
		var maxs = (Controller as UnicycleController).Maxs;
		var result = Controller.TraceTire( Controller.Position, Controller.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		int AttemptsPerTick = 20;

		for ( int i = 0; i < AttemptsPerTick; i++ )
		{
			var pos = Controller.Position + Vector3.Random.Normal * (((float)StuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Controller.Position + Vector3.Up * 5;
			}

			result = Controller.TraceTire( pos, pos );

			if ( !result.StartedSolid )
			{
				Controller.Position = pos;
				return false;
			}
		}

		StuckTries++;

		return true;
	}
}

