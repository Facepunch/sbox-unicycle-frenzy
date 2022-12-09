
using Sandbox;
using Sandbox.UI;
using Sandbox.Utility;

public abstract class BaseEffect
{

	public float Delay;
	public float Duration;
	public float TimeSinceCreated;
	public EasingType Easing;
	public bool Stopped;
	public IJuiceTarget Target;

	public float T => ( TimeSinceCreated - Delay ) / Duration;
	public bool HasStarted => TimeSinceCreated >= Delay;
	public bool HasEnded => ( TimeSinceCreated - Delay ) > Duration || Stopped;

	public virtual void OnStart() { }
	public virtual void OnEnd() { }
	public virtual void OnFrame() { }
	public virtual void OnTick() { }

	public BaseEffect WithEasing( EasingType type )
	{
		Easing = type;
		return this;
	}

	public BaseEffect WithDuration( float duration )
	{
		Duration = duration;
		return this;
	}

	public BaseEffect WithDelay( float delay )
	{
		Delay = delay;
		return this;
	}

	public BaseEffect WithTarget( Entity entity )
	{
		Target = new EntityJuiceTarget( entity );
		return this;
	}

	public BaseEffect WithTarget( Panel panel )
	{
		Target = new PanelJuiceTarget( panel );
		return this;
	}

	public BaseEffect WithTarget( SceneObject obj )
	{
		Target = new SceneObjectJuiceTarget( obj );
		return this;
	}

	public BaseEffect WithTarget( IJuiceTarget target )
	{
		Target = target;
		return this;
	}

	public void Stop()
	{
		Stopped = true;
	}

	protected float Lerp3( float a, float b, float c, float t )
	{

		switch( Easing )
		{
			case EasingType.EaseIn:
				t = Sandbox.Utility.Easing.EaseIn( t );
				break;
			case EasingType.EaseOut:
				t = Sandbox.Utility.Easing.EaseOut( t );
				break;
			case EasingType.EaseInOut:
				t = Sandbox.Utility.Easing.EaseInOut( t );
				break;
			case EasingType.BounceIn:
				t = Sandbox.Utility.Easing.BounceIn( t );
				break;
			case EasingType.BounceOut:
				t = Sandbox.Utility.Easing.BounceOut( t );
				break;
			case EasingType.BounceInOut:
				t = Sandbox.Utility.Easing.BounceInOut( t );
				break;
		}

		if ( t <= 0.5f )
		{
			return a.LerpTo( b, t.LerpInverse( 0f, 0.5f ) );
		}
		else
		{
			return b.LerpTo( c, t.LerpInverse( 0.5f, 1f ) );
		}
	}

}
