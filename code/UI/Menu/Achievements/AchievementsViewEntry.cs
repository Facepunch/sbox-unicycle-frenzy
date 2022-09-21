
using Sandbox.UI;

[UseTemplate]
internal class AchievementsViewEntry : Panel
{

	public Achievement Achievement { get; }

	public AchievementsViewEntry() { }
	public AchievementsViewEntry( Achievement achievement ) 
	{
		Achievement = achievement;
	}



}
