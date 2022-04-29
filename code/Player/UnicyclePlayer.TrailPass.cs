using Sandbox;
using System.Linq;

internal partial class UnicyclePlayer
{

	[ClientRpc]
	public void AddTrailPassExperience( int amount )
	{
		Host.AssertClient();

		if ( !IsLocalPawn ) return;

		var progress = TrailPassProgress.CurrentSeason;
		progress.Experience += amount;
		progress.Save();

		Toaster.Toast( $"+{amount} XP", Toaster.ToastTypes.Award );
	}

	[Event("mapstats.firstcompletion")]
	public void OnFirstCompletion()
	{
		AddTrailPassExperience( 50 );
	}

	[Event("mapstats.ontimeplayed")]
	public void OnTimePlayed( float timePlayed )
	{
		if( (int)timePlayed % 1800 == 0 )
		{
			AddTrailPassExperience( 5 );
		}
	}

	[Event("achievement.set")]
	public void OnAchievementSet( string shortname )
	{
		Host.AssertClient();

		var tpAchievement = TrailPass.Current.Achievements.FirstOrDefault( x => x.AchievementShortName == shortname );
		if ( tpAchievement == null ) return;

		AddTrailPassExperience( tpAchievement.ExperienceGranted );
	}

}
