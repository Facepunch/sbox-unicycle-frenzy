﻿
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
	public int CheckpointsReached { get; set; }
	public int TotalCheckpoints { get; set; }
	public CheckPointZone CurrentCheckpoint { get; set; }

	protected override void OnAwake()
	{
		base.OnAwake();

		Local = this;

		TotalCheckpoints = Scene.Children
								  .Select( x => x.Components.Get<BaseZone>() )
								  .Where( x => x != null && x.IsCheckPoint )
								  .Count();
	}

	public void ResetTimer()
	{
		State = TimerStates.Idle;
		TimeSinceStart = 0;
		FinishTime = 0;

		foreach ( var checkpoint in Scene.Children )
		{
			var check = checkpoint.Components.Get<CheckPointZone>();
			if ( check == null ) continue;

			check.CurrentCheckpoint = false;

			var start = checkpoint.Components.Get<StartZone>();
			if ( start == null ) continue;

			start.RunStarted = false;
		}

		foreach ( var checkpoint in Scene.Children )
		{
			var mapsetting = Scene.GetAllComponents<MapSettings>().FirstOrDefault();
			if ( mapsetting == null ) return;
			mapsetting.frenzyLetterList.Clear();

			var frenzy = checkpoint.Components.Get<FrenzyPickUp>();
			if ( frenzy == null ) continue;

			frenzy.OnRestart();
		}
	}

	public void ResetCheckpoints()
	{
		State = TimerStates.Finished;
		CurrentCheckpoint = null;

		foreach ( var checkpoint in Scene.Children )
		{

			var start = checkpoint.Components.Get<StartZone>();
			if ( start == null ) continue;

			start.RunStarted = false;

			var check = checkpoint.Components.Get<CheckPointZone>();
			if ( check == null ) continue;

			check.CurrentCheckpoint = false;
		}
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

		var mapsetting = Scene.GetAllComponents<MapSettings>().FirstOrDefault();
		if ( mapsetting == null ) return;
		mapsetting.SetBestTime( (float)FinishTime );
		mapsetting.MedalCheck( (float)FinishTime );
		mapsetting.OnFinish();
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

		if(startZone.RunStarted)
		{
			if ( CurrentCheckpoint == null )
			{
				position = startZone.Transform.Local.PointToWorld( startZone.Bounds.Center );
				forward = startZone.Transform.Rotation.Forward.EulerAngles;
				Log.Info( "No current checkpoint" );
				return true;
			}

			var check = CurrentCheckpoint.Components.Get<CheckPointZone>();
			if ( check.CurrentCheckpoint )
			{
				position = check.Transform.Local.PointToWorld( check.Bounds.Center );
				forward = check.Transform.Rotation.Forward.EulerAngles;
				return true;
			}
			
		}
		else
		{
			position = startZone.Transform.Local.PointToWorld( startZone.Bounds.Center );
			forward = startZone.Transform.Rotation.Forward.EulerAngles;
		}

		return true;
	}

}
