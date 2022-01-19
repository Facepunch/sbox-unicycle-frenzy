using Sandbox;
using System.Linq;

internal partial class UnicyclePlayer
{

	[Net, Predicted]
	public TimerState TimerState { get; set; }
	[Net, Predicted]
	public TimeSince TimeSinceStart { get; set; }
	[Net, Change]
	public float BestTime { get; set; } = defaultBestTime;

	public int SessionRank
	{
		get
		{
			var rank = 1;
			foreach( var ent in Entity.All )
			{
				if ( !ent.IsValid() || ent == this ) continue;
				if ( ent is not UnicyclePlayer pl ) continue;
				if ( pl.CourseIncomplete ) continue;
				if ( pl.BestTime > BestTime ) continue;
				rank++;
			}
			return rank;
		}
	}

	public bool CourseIncomplete => BestTime == defaultBestTime;

	private const float defaultBestTime = 3600f; // easier to check for this than sorting out 0/default

	public void EnterStartZone()
	{
		ResetTimer();
	}

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;

		AddAttempts();
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
				if( CourseIncomplete )
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

			Celebrate();
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

	public void TrySetCheckpoint( Checkpoint checkpoint, bool overridePosition = false )
	{
		Host.AssertServer();

		if ( Checkpoints.Contains( checkpoint ) )
		{
			if ( overridePosition )
			{
				for( int i = Checkpoints.Count - 1; i >= 0; i-- )
				{
					if ( Checkpoints[i] != checkpoint )
						Checkpoints.RemoveAt( i );
				}
			}
			return;
		}

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

	private void OnBestTimeChanged()
	{
		if ( !IsLocalPawn ) return;
		MapStats.Local.SetBestTime( BestTime );
	}

	[ClientRpc]
	private void AddAttempts()
	{
		if ( !IsLocalPawn ) return;
		MapStats.Local.AddAttempt();
	}

	[ClientRpc]
	private void Celebrate()
	{
		if ( !IsLocalPawn ) return;

		Particles.Create( "particles/finish/finish_effect.vpcf" );
		Sound.FromScreen( "course.complete" );

		MapStats.Local.AddCompletion();
	}

	[ServerCmd( "uf_nextcp" )]
	private static void GotoNextCheckpoint()
	{
		if ( !ConsoleSystem.Caller.IsValid() || ConsoleSystem.Caller.Pawn is not UnicyclePlayer pl ) return;

		var currentCp = pl.Checkpoints.LastOrDefault();
		var targetCp = currentCp == null ? 1 : currentCp.Number + 1;
		var nextCp = Entity.All.FirstOrDefault( x => x is Checkpoint cp && cp.Number == targetCp ) as Checkpoint;
		if ( nextCp == null ) return;
		pl.TimerState = TimerState.InStartZone;
		pl.TrySetCheckpoint( nextCp, true );
		pl.GotoBestCheckpoint();
	}

	[ServerCmd( "uf_prevcp" )]
	private static void GotoPreviousCheckpoint()
	{
		if ( !ConsoleSystem.Caller.IsValid() || ConsoleSystem.Caller.Pawn is not UnicyclePlayer pl ) return;

		var currentCp = pl.Checkpoints.LastOrDefault();
		var targetCp = currentCp == null ? 0 : currentCp.Number - 1;
		var nextCp = Entity.All.FirstOrDefault( x => x is Checkpoint cp && cp.Number == targetCp ) as Checkpoint;
		if ( nextCp == null ) return;
		pl.TimerState = TimerState.InStartZone;
		pl.TrySetCheckpoint( nextCp, true );
		pl.GotoBestCheckpoint();
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

