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

	private Particles Crown;

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

	public void ResetBestTime()
	{
		BestTime = defaultBestTime;
	}

	public void EnterStartZone()
	{
		ResetTimer();
	}

	public void StartCourse()
	{
		TimeSinceStart = 0;
		TimerState = TimerState.Live;
		Velocity = Velocity.ClampLength( 240 );

		AddAttempts();
	}

	public async void CompleteCourse()
	{
		TimerState = TimerState.Finished;

		if( IsServer )
		{
			ClearCheckpoints();

			var formattedTime = CourseTimer.FormattedTimeMsf( TimeSinceStart );

			SetAchievementOnClient( To.Single( Client ), "uf_unicyclist" );

			var medalThresholds = Entity.All.FirstOrDefault( x => x is AchievementMedals ) as AchievementMedals;
			if ( medalThresholds.IsValid() )
			{
				if( TimeSinceStart <= medalThresholds.Bronze )
					SetAchievementOnClient( To.Single( Client ), "uf_bronze", Global.MapName );
				if ( TimeSinceStart <= medalThresholds.Silver )
					SetAchievementOnClient( To.Single( Client ), "uf_silver", Global.MapName );
				if ( TimeSinceStart <= medalThresholds.Gold )
					SetAchievementOnClient( To.Single( Client ), "uf_gold", Global.MapName );
			}

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

				var scoreResult = await GameServices.SubmitScore( Client.PlayerId, BestTime );

				// we can print new rank, old rank, improvement, etc from score result
			}

			Celebrate();
		}
	}

	public void ResetTimer()
	{
		TimerState = TimerState.InStartZone;
		TimeSinceStart = 0;

		if ( IsServer )
		{
			ClearCheckpoints();

			if (Global.MapName.EndsWith("uf_tutorial"))
            {
				ResetTutorial();
			}
		}
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

	[Event.Tick.Server]
	private void EnsureCrown()
	{
		var needsCrown = !Fallen 
			&& !CourseIncomplete
			&& SessionRank == 1
			&& Citizen.IsValid();

		if( needsCrown && Crown == null )
		{
			Crown = Particles.Create( "particles\\crown\\current_session\\current_session_crown.vpcf" );
			Crown.SetEntityAttachment( 0, Citizen, "hat", true );
		}
		else if( !needsCrown && Crown != null )
		{
			Crown?.Destroy();
			Crown = null;
		}
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
		pl.TeleportToCheckpoint( nextCp );
	}

	[ServerCmd( "uf_prevcp" )]
	private static void GotoPreviousCheckpoint()
	{
		if ( !ConsoleSystem.Caller.IsValid() || ConsoleSystem.Caller.Pawn is not UnicyclePlayer pl ) return;

		var currentCp = pl.Checkpoints.LastOrDefault();
		var targetCp = currentCp == null ? 0 : currentCp.Number - 1;
		var nextCp = Entity.All.FirstOrDefault( x => x is Checkpoint cp && cp.Number == targetCp ) as Checkpoint;
		if ( nextCp == null ) return;
		pl.TeleportToCheckpoint( nextCp );
	}

	private async void TeleportToCheckpoint( Checkpoint cp )
	{
		TimerState = TimerState.InStartZone;
		TrySetCheckpoint( cp, true );
		GotoBestCheckpoint();

		await System.Threading.Tasks.Task.Delay( 100 );

		TimerState = TimerState.InStartZone;
	}

}

public enum TimerState
{
	InStartZone,
	Live,
	Finished
}

