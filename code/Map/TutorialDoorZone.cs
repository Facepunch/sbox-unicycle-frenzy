internal class TutorialDoorZone : BaseZone
{
	[Property]
	public GameObject RightDoor { get; set; }
	[Property]
	public GameObject LeftDoor { get; set; }
	[Property]
	public SoundEvent DoorOpenSound { get; set; }

	private UnicycleController overlappingPlayer;
	private bool doorsOpen = false;
	private bool openingDoors = false;

	protected override void OnAwake()
	{
		base.OnAwake();

		IsCheckPoint = false;

		if ( !Scene.GetAllComponents<TutorialMap>().FirstOrDefault().ShowTutorial )
		{
			openingDoors = true;
		}
	}
	protected override void OnUpdate()
	{
		base.OnUpdate();
		if ( doorsOpen ) return;
		if ( openingDoors ) OpenDoors();

		if ( overlappingPlayer == null ) return;

		if ( !overlappingPlayer.Dead && overlappingPlayer.CurrentSpeed.AlmostEqual( 0f, 0.0001f ) && !openingDoors )
		{
			openingDoors = true;
		}		
	}

	protected override void OnPlayerEnter( UnicycleController player )
	{
		overlappingPlayer = player;
	}

	protected override void OnPlayerExit( UnicycleController player )
	{
		overlappingPlayer = null;
	}

	void OpenDoors()
	{
		RightDoor.Transform.Rotation = Angles.Lerp( RightDoor.Transform.Rotation.Angles(), new Angles(0, -180, 0), 3f * Time.Delta ).ToRotation();
		LeftDoor.Transform.Rotation = Angles.Lerp( LeftDoor.Transform.Rotation.Angles(), new Angles( 0, 180, 0 ), 3f * Time.Delta ).ToRotation();

		if ( RightDoor.Transform.Rotation.Angles().yaw.AlmostEqual( -180f, 0.1f ) )
		{
			openingDoors = false;
			doorsOpen = true;
		}
	}
}
