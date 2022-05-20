
using Sandbox;
using System.ComponentModel.DataAnnotations;

[EditorModel("models/leaderboard_post.vmdl")]
[Library( "uf_leaderboard", Description = "Shows a leaderboard in-game" )]
[Display( Name = "Leaderboard Post", GroupName = "Unicycle Frenzy", Description = "Shows a leaderboard in-game" )]
internal class LeaderboardPost : ModelEntity
{
	public enum ModelType
	{
		Metal,
		Stone,
		Wood
	}

	/// <summary>
	/// Movement type of the door.
	/// </summary>
	[Property("model_type", Title = "Model Type")]
	public ModelType ModelTypeList { get; set; } = ModelType.Metal;


	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		EnableAllCollisions = true;

		if (ModelTypeList == ModelType.Wood)
		{
			SetModel("models/leaderboard_post_wood.vmdl");
		}

		else if (ModelTypeList == ModelType.Metal)
		{
			SetModel("models/leaderboard_post.vmdl");
		}

		else if (ModelTypeList == ModelType.Stone)
		{
			SetModel("models/leaderboard_post_stone.vmdl");
		}
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		new LeaderboardSign( this );
	}
}

