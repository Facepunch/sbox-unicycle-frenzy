
using Sandbox;
using Editor;
using System.ComponentModel.DataAnnotations;
using System.Linq;

[Library("uf_checkpoint", Description = "Defines a checkpoint where the player will respawn after falling")]
[EditorModel( "models/checkpoint_platform_hammer.vmdl", FixedBounds = true)]
[Display( Name = "Player Checkpoint", GroupName = "Unicycle Frenzy", Description = "Defines a checkpoint where the player will respawn after falling." )]
[HammerEntity]
internal partial class Checkpoint : ModelEntity
{

	[Net, Property]
	public bool IsStart { get; set; }
	[Net, Property]
	public bool IsEnd { get; set; }
	[Net, Property]
	public int Number { get; set; }

	[Net, Property( "No Camera Collide", "The Unicycle camera will maintain its position when touching this prop" )]
	public bool NoCameraCollide { get; set; }

	[Net]
	public bool IsMetalLargeFrame { get; set; }
	
	[Net]
	public bool IsMetalSmallFrame { get; set; }

	public bool BlockingView = false;

	private ModelEntity flag;

	public enum ModelType
	{
		Dev,
		Metal,
		Stone,
		Wood,
		LargeMetalFrame,
		SmallMetalFrame
	}

	/// <summary>
	/// Movement type of the door.
	/// </summary>
	[Property("model_type", Title = "Model Type")]
	public ModelType ModelTypeList { get; set; } = ModelType.Dev;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableAllCollisions = true;
		EnableTouch = true;

		if (ModelTypeList == ModelType.Dev)
		{
			SetModel("models/checkpoint_platform.vmdl");
		}

		else if (ModelTypeList == ModelType.Metal)
		{
			SetModel("models/checkpoint_platform_metal.vmdl");
		}
		
		else if (ModelTypeList == ModelType.Stone)
		{
			SetModel("models/checkpoint_platform_stone.vmdl");
		}

		else if (ModelTypeList == ModelType.Wood)
		{
			SetModel("models/checkpoint_platform_wood.vmdl");
		}
		else if ( ModelTypeList == ModelType.LargeMetalFrame )
		{
			SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_large.vmdl" );
			IsMetalLargeFrame = true;
		}
		else if ( ModelTypeList == ModelType.SmallMetalFrame )
		{
			SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_small.vmdl" );
			IsMetalSmallFrame = true;
		}

		SetupPhysicsFromModel( PhysicsMotionType.Static );

		var bounds = PhysicsBody.GetBounds();
		var extents = ( bounds.Maxs - bounds.Mins ) * 0.5f;

		var trigger = new BaseTrigger();
		trigger.SetParent( this, null, Transform.Zero );
		trigger.SetupPhysicsFromAABB( PhysicsMotionType.Static, -extents.WithZ( 0 ), extents.WithZ( 128 ) );
		trigger.Transmit = TransmitType.Always;
		trigger.EnableTouchPersists = true;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		
		if ( IsMetalLargeFrame )
		{
			var flagAttachment = GetAttachment( "Flag" );

			flag = new ModelEntity( "" );
			flag.Position = flagAttachment.Value.Position;
			flag.Rotation = flagAttachment.Value.Rotation;

			if ( this.IsStart )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_large_sign.vmdl" );
				flag.SetMaterialGroup( "Green" );
			}

			if ( this.IsEnd )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_large_sign.vmdl" );
				flag.SetMaterialGroup( "Checker" );
			}
		}
		else if ( IsMetalSmallFrame )
		{
			var flagAttachment = GetAttachment( "Flag" );

			flag = new ModelEntity("");
			flag.Position = flagAttachment.Value.Position;
			flag.Rotation = flagAttachment.Value.Rotation;

			if ( this.IsStart )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_small_sign.vmdl" );
				flag.SetMaterialGroup( "Green" );
			}

			if ( this.IsEnd )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_small_sign.vmdl" );
				flag.SetMaterialGroup( "Checker" );
			}
		}
		else
		{
			var flagAttachment = GetAttachment( "Flag" );

			flag = new ModelEntity( "models/flag/flag_pole.vmdl" );
			flag.Position = flagAttachment.Value.Position;
			flag.Rotation = flagAttachment.Value.Rotation;

			if ( this.IsStart )
			{
				flag.SetModel( "models/flag/flag.vmdl" );
				flag.SetMaterialGroup( "Green" );
			}

			if ( this.IsEnd )
			{
				flag.SetModel( "models/flag/flag.vmdl" );
				flag.SetMaterialGroup( "Checker" );
			}
		}
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( other is not UnicyclePlayer pl ) return;
		if ( !CanPlayerCheckpoint( pl )) return;

		pl.TrySetCheckpoint( this );

		if ( IsEnd ) pl.CompleteCourse();
		else if ( IsStart ) pl.EnterStartZone();
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not UnicyclePlayer pl ) return;
		if ( !IsStart ) return;

		pl.StartCourse();
	}

	private bool CanPlayerCheckpoint( UnicyclePlayer pl )
	{
		if ( pl.TimerState == TimerState.Live && IsMetalLargeFrame || IsMetalSmallFrame ) return true;
		if ( pl.GroundEntity == null ) return false;
		if ( pl.Fallen  ) return false;
		if ( pl.TimerState != TimerState.Live ) return false;
		
		return true;
	}

	private bool active;
	[Event.Client.Frame]
	private void OnFrame()
	{
		if ( Local.Pawn is not UnicyclePlayer pl ) return;
		if ( this.IsEnd || this.IsStart ) return;

		var isLatestCheckpoint = pl.Checkpoints.LastOrDefault() == this;

		if ( !active && isLatestCheckpoint )
		{
			active = true;
			if ( IsMetalLargeFrame )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_large_sign.vmdl" );
			}
			else if ( IsMetalSmallFrame )
			{
				flag.SetModel( "models/checkpoint_metal_frame/checkpoint_metal_frame_small_sign.vmdl" );
			}
			else
			{
				flag.SetModel( "models/flag/flag.vmdl" );

				Juice.Scale( 1f, 1.25f, 1f )
					.WithDuration( .5f )
					.WithEasing( EasingType.BounceOut )
					.WithTarget( flag );
			}
		}
		else if ( active && !isLatestCheckpoint )
		{
			active = false;
			if ( IsMetalLargeFrame || IsMetalSmallFrame )
			{
				flag.SetModel( "" );
			}
			else
			{
				flag.SetModel( "models/flag/flag_pole.vmdl" );
			}
		}
	}

	public void GetSpawnPoint( out Vector3 position, out Rotation rotation )
	{
		position = Position;
		rotation = Rotation;
	}

}
