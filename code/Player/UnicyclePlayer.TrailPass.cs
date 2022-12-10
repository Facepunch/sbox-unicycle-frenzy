using Sandbox;
using System.Linq;

internal partial class UnicyclePlayer
{

	[ClientRpc]
	public void AddTrailPassExperience( int amount )
	{
		Game.AssertClient();

		if ( !IsLocalPawn ) return;

		var progress = TrailPassProgress.Current;
		progress.Experience += amount;
		progress.Save();

		Toaster.Toast( $"+{amount} XP", Toaster.ToastTypes.Award );
	}

	[Event( "mapstats.firstcompletion" )]
	public void OnFirstCompletion()
	{
		AddTrailPassExperience( 50 );
	}

	[Event( "mapstats.ontimeplayed" )]
	public void OnTimePlayed( float timePlayed )
	{
		if ( (int)timePlayed % 1800 == 0 )
		{
			AddTrailPassExperience( 5 );
		}
	}

	[Event( "achievement.set" )]
	public void OnAchievementSet( string shortname )
	{
		Game.AssertClient();

		if ( TrailPass.Current.TryGetAchievement( shortname, out var ach ) )
		{
			AddTrailPassExperience( ach.ExperienceGranted );
		}
	}

}
