
using Sandbox;
using System.Linq;

internal enum TimerStates
{
	Idle,
	Started,
	Finished
}

internal class CourseTimer : Component
{

	public static CourseTimer Local;

	public TimerStates State { get; set; }
	public RealTimeSince TimeSinceStart { get; set; }
	public double FinishTime { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		Local = this;
	}

	public void ResetTimer()
	{
		State = TimerStates.Idle;
		TimeSinceStart = 0;
		FinishTime = 0;
	}

	public void StartTimer()
	{
		TimeSinceStart = 0;
		State = TimerStates.Started;
	}

	public void FinishTimer()
	{
		if ( State != TimerStates.Started ) return;

		State = TimerStates.Finished;
		FinishTime = (double)TimeSinceStart;
	}

	public bool TryFindCheckpoint( out Vector3 position, out Angles forward )
	{
		position = default;
		forward = default;

		var startZone = Scene.Children.FirstOrDefault( x => x.Components.Get<StartZone>() != null ).Components.Get<StartZone>();
		if ( startZone == null )
		{
			return false;
		}

		position = startZone.Transform.Local.PointToWorld( startZone.Bounds.Center );
		forward = startZone.Transform.Rotation.Forward.EulerAngles;

		return true;
	}

}
