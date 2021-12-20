using Sandbox;
using System.Linq;

internal partial class UnicyclePlayer
{

	[Net, Predicted]
	public TimerState TimerState { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceStart { get; set; }
	[Net]
	public float BestTime { get; set; } = float.MaxValue;

	public void EnterStartZone()
	{
		ResetTimer();
	}

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;
	}

	public void CompleteCourse()
	{
		TimerState = TimerState.Finished;

		if( IsServer )
		{
			ClearCheckpoints();

			var formattedTime = CourseTimer.FormattedTimeMsf( TimeSinceStart );

			if ( TimeSinceStart < BestTime )
			{
				if( BestTime == float.MaxValue )
				{
					UfChatbox.AddCustom( To.Everyone, $"{Client.Name} completed the course in {formattedTime}", "timer-msg" );
				}
				else
				{
					var improvement = CourseTimer.FormattedTimeMsf( BestTime - TimeSinceStart );
					UfChatbox.AddCustom( To.Everyone, $"{Client.Name} completed the course in {formattedTime}, improving by {improvement}!", "timer-msg" );
				}

				BestTime = TimeSinceStart;
			} 
			else
			{
				UfChatbox.AddCustom( To.Everyone, $"{Client.Name} completed the course in {formattedTime}, but their best time was {CourseTimer.FormattedTimeMsf(BestTime)}", "timer-msg" );
			}
		}
	}

	public void ResetTimer()
	{
		TimerState = TimerState.InStartZone;
		TimeSinceStart = 0;

		if ( IsServer ) ClearCheckpoints();
	}

	public void ClearCheckpoints()
	{
		Host.AssertServer();

		Checkpoints.Clear();
	}

	public void TrySetCheckpoint( Checkpoint checkpoint )
	{
		Host.AssertServer();

		if ( Checkpoints.Contains( checkpoint ) ) return;
		Checkpoints.Add( checkpoint );
	}

	public void GotoBestCheckpoint()
	{
		Host.AssertServer();

		var cp = Checkpoints.LastOrDefault();
		if ( !cp.IsValid() )
		{
			cp = Entity.All.FirstOrDefault( x => x is Checkpoint c && c.IsStart ) as Checkpoint;
			if ( cp == null ) return;
		}

		cp.GetSpawnPoint( out Vector3 position, out Rotation rotation );

		Position = position + Vector3.Up * 5;
		Rotation = rotation;
		Velocity = Vector3.Zero;

		SetRotationOnClient( Rotation );
		ResetInterpolation();
		ResetMovement();
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

