﻿using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class AchievementUnlocked : Panel
{

	private TimeSince timeSinceDisplayed;

	public Panel Icon { get; set; }
	public Label DisplayName { get; set; }
	public Label Experience { get; set; }

	public void Display( Achievement achievement )
	{
		Icon.Style.SetBackgroundImage( achievement.Thumbnail );
		DisplayName.Text = achievement.DisplayName;
		Experience.Text = string.Empty;

		if ( TrailPass.Current.TryGetAchievement( achievement.ShortName, out var ach ) )
		{
			Experience.Text = $"+{ach.ExperienceGranted}xp";
		}

		SetClass( "open", true );
		timeSinceDisplayed = 0;

		Sound.FromScreen( "sounds/ui/achievement.unlocked.sound", .9f, 1f );
	}

	public override void Tick()
	{
		base.Tick();

		if ( HasClass( "open" ) && timeSinceDisplayed > 6f )
			RemoveClass( "open" );
	}

	[Event( "achievement.set" )]
	public void OnAchievementSet( string shortname )
	{
		var ach = Achievement.All.FirstOrDefault( x => x.ShortName == shortname );
		if ( ach == null ) return;

		Display( ach );
	}

}
