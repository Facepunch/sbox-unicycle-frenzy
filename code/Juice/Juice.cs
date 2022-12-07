using Sandbox;
using System.Collections.Generic;

public static class Juice
{

	private static List<BaseEffect> effects = new();

	// todo: 3 lerp kinda sucks
	// also probably can simplify, scale/alpha are basically doing same thing to different property

	public static ScaleEffect Scale( float a, float b, float c )
	{
		var effect = new ScaleEffect( a, b, c );
		effect.TimeSinceCreated = 0;
		effects.Add( effect );
		return effect;
	}

	public static AlphaEffect Alpha( float a, float b, float c )
	{
		var effect = new AlphaEffect( a, b, c );
		effect.TimeSinceCreated = 0;
		effects.Add( effect );
		return effect;
	}

	[Event.Tick]
	private static void OnTick()
	{
		for( int i = effects.Count - 1; i >= 0; i-- )
		{ 
			if( effects[i].HasEnded )
			{
				effects.RemoveAt( i );
				continue;
			}

			if( effects[i].Target == null || !effects[i].Target.IsValid() )
			{
				effects.RemoveAt( i );
				continue;
			}

			effects[i].TimeSinceCreated += Time.Delta;

			if ( effects[i].Delay > effects[i].TimeSinceCreated )
			{
				continue;
			}

			effects[i].OnTick();
		}
	}
	
	[Event.Client.Frame]
	private static void OnFrame()
	{
		foreach ( var effect in effects )
		{
			if ( effect.HasEnded ) continue;
			effect.OnFrame();
		}
	}

}

public enum EasingType
{
	Linear,
	EaseIn,
	EaseOut,
	EaseInOut,
	BounceIn,
	BounceOut,
	BounceInOut
}
