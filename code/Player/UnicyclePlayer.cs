using Sandbox.Utility;

internal partial class UnicyclePlayer : Sandbox.Player
{

    [Net]
    public AnimatedEntity Citizen { get; set; }
    [Net]
    public UnicycleEntity Unicycle { get; set; }
    [Net]
    public IList<Checkpoint> Checkpoints { get; set; }
	[Net]
	public string Avatar { get; set; }

    public const float MaxRenderDistance = 300f;
    public const float RespawnDelay = 3f;

    private TimeSince TimeSinceDied;
    private ClothingContainer Clothing;
    private UfNametag Nametag;
    private JumpIndicator JumpIndicator;
    private Particles SpeedParticle;

	UnicycleCamera UnicycleCamera = new UnicycleCamera();


	public override void Respawn()
    {
        base.Respawn();

        SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -12, -12, 0 ), new Vector3( 12, 12, 80 ) );

		EnableDrawing = false;
        EnableAllCollisions = true;

        Unicycle = new UnicycleEntity();
        Unicycle.SetParent( this, null, Transform.Zero );

        Citizen = new AnimatedEntity( "models/citizen/citizen.vmdl" );
		Citizen.SetParent( this, null, Transform.Zero );
        Citizen.SetAnimGraph( "models/citizen_unicycle_frenzy.vanmgrph" );

        Controller ??= new UnicycleController();

		Clothing ??= new();
		Clothing.LoadFromClient( Client );
        Clothing.DressEntity( Citizen );
		Avatar = Client.GetClientData( "avatar" );

		Tags.Add( "player" );

		ResetMovement();
        GotoBestCheckpoint();
    }

	public override void ClientSpawn()
    {
        base.ClientSpawn();

        Nametag = new( this );

		if ( IsLocalPawn )
		{
			SpeedParticle = Particles.Create( "particles/player/speed_lines.vpcf" );
			JumpIndicator = new( this );
		}
    }

    public override void OnKilled()
    {
        base.OnKilled();

        TimeSinceDied = 0;
        EnableAllCollisions = false;
        EnableDrawing = false;

        Unicycle?.Delete();
        Citizen?.Delete();

        RagdollOnClient();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Crown?.Destroy();
        JumpIndicator?.Delete();
		Nametag?.Delete();
	}

    public override void Simulate( IClient cl )
    {
		// don't simulate when spectating somebody
        if ( SpectateTarget.IsValid() ) return;

		if ( !Fallen )
        {
            var controller = GetActiveController();
            controller?.Simulate( cl, this );

			UpdateAnimation();

            if ( GetActiveController() == DevController )
            {
                ResetMovement();
                ResetTimer();
            }

            if ( Game.IsServer && InputActions.Spray.Pressed() )
            {
                Spray();
            }
        }

        if ( Fallen )
        {
            if ( Game.IsServer && TimeSinceDied > RespawnDelay )
                Respawn();
        }

        if ( InputActions.RestartAtCheckpoint.Pressed() || InputActions.RestartCourse.Pressed() )
        {
            if ( LifeState != LifeState.Dead )
                AddRespawnOnClient();

            if ( InputActions.RestartCourse.Pressed() )
                ResetTimer();

            Fall( false );
            TimeSinceDied = Math.Max( TimeSinceDied, RespawnDelay - .5f );
        }
    }

	private float TimePlayedCounter;
	public override void FrameSimulate( IClient cl )
	{
		base.FrameSimulate( cl );

		UpdateCamera();
		
		if ( !IsLocalPawn ) return;

		TimePlayedCounter += Time.Delta;
		if ( TimePlayedCounter < 1 ) return;

		TimePlayedCounter = 0;
		MapStats.Local.AddTimePlayed( 1f );
	}

	public void UpdateCamera()
	{
		UnicycleCamera.Update();

		Camera.Rotation *= Rotation.From( Tilt * .015f );

		BaseCameraModifier.PostCameraSetup();
	}

	[Event.Client.Frame]
    private void UpdateRenderAlpha()
    {
        if ( Game.LocalPawn == this ) return;
        if ( !Citizen.IsValid() || !Unicycle.IsValid() ) return;

        var a = GetRenderAlpha();
		Citizen.SetRenderAlphaRecursive( a );
		Unicycle.SetRenderAlphaRecursive( a );
    }

	private float targetSpeedParticle;
	private float currentSpeedParticle;
	
	[Event.Client.Frame]
	private void UpdateSpeedParticle()
	{
		if ( SpeedParticle == null ) return;

		// ?
		var spd = Math.Min( Velocity.Length, 800 );
		targetSpeedParticle = spd < 400 || Fallen ? 0 : (spd - 400) / 400f;

		var lerpSpd = targetSpeedParticle == 0 ? 6 : 1;

		currentSpeedParticle = currentSpeedParticle.LerpTo( targetSpeedParticle, Time.Delta * lerpSpd );
		SpeedParticle.SetPosition( 1, new Vector3( currentSpeedParticle, 0, 0 ) );
	}

	public float GetRenderAlpha()
	{
		if ( !Game.LocalPawn.IsValid() ) return 1f;

		var dist = Game.LocalPawn.Position.Distance( Position );
		var a = 1f - dist.LerpInverse( MaxRenderDistance, MaxRenderDistance * .1f );
		a = Math.Max( a, .15f );
		a = Easing.EaseOut( a );

		return a;
	}

	[ConCmd.Server]
    public static void ServerCmd_SetSpectateTarget( int entityId )
    {
        if ( !ConsoleSystem.Caller.IsValid() ) return;

        var caller = ConsoleSystem.Caller.Pawn as UnicyclePlayer;
        if ( !caller.IsValid() ) return;

        var ent = Entity.FindByIndex( entityId ) as UnicyclePlayer;
        if ( !ent.IsValid() ) ent = null;

        caller.SpectateTarget = ent == caller ? null : ent;
    }

    [ClientRpc]
    private void AddRespawnOnClient()
    {
        if ( !IsLocalPawn ) return;
        MapStats.Local.AddRespawn();
    }

    [ClientRpc]
    private void SetAchievementOnClient( string shortname, string map = null )
    {
        Achievement.Set( Client.SteamId, shortname, map );
    }

    private TimeSince timeSinceSpray;
    private void Spray()
    {
        if ( GroundEntity == null || Fallen ) return;
        if ( timeSinceSpray < 3f ) return;
        timeSinceSpray = 0;

		var sprayPart = Client.Components.Get<CustomizationComponent>().GetEquippedPart( PartType.Spray );
		var decal = sprayPart.Decal;
		if ( decal == null ) return;

		var tr = Trace.Ray( Position + Vector3.Up, Position + Vector3.Down * 100f )
			.WorldOnly()
			.Run();
		
		Decal.Place( decal, tr );
		SprayEffect(sprayPart.ColorTint);
		Sound.FromWorld( sprayPart.DecalSound, tr.HitPosition + Vector3.Up );
    }

	[ClientRpc]
	private void SprayEffect(Color tint)
	{
		var prtSpray = Particles.Create( "particles/player/spray.vpcf",Position );
		prtSpray.SetPosition( 1, tint );
	}

}

