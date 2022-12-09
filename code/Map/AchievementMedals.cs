
using Sandbox;
using Editor;
using System.ComponentModel.DataAnnotations;

[Library("uf_achievement_medals")]
[Display( Name = "Unicycle Frenzy Medals", GroupName = "Unicycle Frenzy", Description = "Set the time for achievement medals." )]
[EditorSprite( "materials/editor/achievement_medals.vmat" )]
[HammerEntity]
internal partial class AchievementMedals : Entity
{

	public AchievementMedals()
	{
		Transmit = TransmitType.Always;
	}

	[Property("gold_threshold", "Time (in seconds) to achieve the gold medal for this map")]
	[Net]
	public float Gold { get; set; }
	[Property( "silver_threshold", "Time (in seconds) to achieve the silver medal for this map" )]
	[Net]
	public float Silver { get; set; }
	[Property( "bronze_threshold", "Time (in seconds) to achieve the bronze medal for this map" )]
	[Net]
	public float Bronze { get; set; }

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Event.Run( "achievement.medals.spawned" );
	}

}
