using Sandbox;
using Sandbox.UI;
using System.Linq;

[UseTemplate]
internal class AchievementUnlocked : Panel
{

	public Panel Canvas { get; set; }

	public void Display( Achievement achievement )
	{
		Canvas.AddChild( new AchievementUnlockedEntry( achievement ) );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Input.Pressed( InputButton.Jump ) )
		{
			Display( Rand.FromArray( Achievement.All.ToArray() ) );
		}
	}

	[Event( "achievement.set" )]
	public void OnAchievementSet( string shortname )
	{
		var ach = Achievement.All.FirstOrDefault( x => x.ShortName == shortname );
		if ( ach == null ) return;

		Display( ach );
	}

}
