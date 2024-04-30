
using Sandbox;
using Sandbox.Utility;
using System;
using System.Numerics;

internal class UnicycleController : Component
{

	public static UnicycleController Local;
	public static bool NoFall => false;


	[Property]
	public SoundEvent FallSound { get; set; }
	[Property]
	public SoundEvent RespawnSound { get; set; }
	[Property]
	public SoundEvent PedalSound { get; set; }
	[Property]
	public SoundEvent PerfectPedalSound { get; set; }
	[Property]
	public SoundEvent JumpSound { get; set; }

	public float PedalTime => .45f;
	public float PedalResetAfter => 1.5f;
	public float PedalResetTime => .55f;
	public float MinPedalStrength => 8f;
	public float MaxPedalStrength => 35f;
	public float MinJumpStrength => 200f;
	public float MaxJumpStrength => 375f;
	public float MaxJumpStrengthTime => .8f;
	public float PerfectPedalBoost => 50f;
	public float PerfectPedalTimeframe => .25f;
	public float MaxLean => 30f;
	public float LeanSafeZone => 5f;
	public float LeanSpeed => 60f;
	public float TipSpeed => 1.75f;
	public float SlopeTipSpeed => 3f;
	public float GroundTurnSpeed => 1.4f;
	public float AirTurnSpeed => 2f;
	public float SlopeSpeed => 800f;
	public float BrakeStrength => 2f;
	public float StopSpeed => 10f;
	public float MaxAirTurnSpeed => 35f;
	public float ForwardVelocityTilt => 3f;
	public float RightVelocityTilt => 1.5f;
	public int MaxHorizontalSpeed => 800;

	public float CurrentSpeed => Velocity.Length;

	public bool Dead { get; private set; }

	public bool ForceFall { get; set; }

	public Vector3 Mins => new( -1, -1, 0 );
	public Vector3 Maxs => new( 1, 1, 16 );
	public Vector3 Position
	{
		get => Transform.Position;
		set => Transform.Position = value;
	}
	public Rotation Rotation
	{
		get => Transform.Rotation;
		set => Transform.Rotation = value;
	}

	public Vector3 Velocity { get; set; }
	public Vector3 PrevVelocity { get; set; }
	public Vector3 BaseVelocity { get; set; }

	// These were previously predicted/networked vars:
	public TimeSince TimeSinceNotGrounded { get; private set; }
	public float TimeSincePedalStart { get; private set; }
	public Angles Tilt { get; private set; }
	public Angles JumpTilt { get; private set; }
	public Angles PrevJumpTilt { get; private set; }
	public float PedalPosition { get; private set; }
	public float TimeSinceJumpDown { get; private set; }
	public Rotation TargetForward { get; private set; }

	private string groundSurface;
	Vector3 GroundNormal;
	GameObject Ground;
	bool PrevGrounded;

	AchievementManager AchievementManager;

	protected override void OnAwake()
	{
		base.OnAwake();

		Local = this;
	}

	protected override void OnStart()
	{
		base.OnStart();

		Respawn();

		AchievementManager = new AchievementManager();
		AchievementManager.Fetch();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Input.EscapePressed )
		{
			GameManager.ActiveScene.LoadFromFile( "scenes/menu.scene" );
		}

		if ( Input.Pressed( "Reset" ) )
		{
			Respawn();
		}
		if ( Input.Pressed( "Restart" ) )
		{
			var courseTimer = CourseTimer.Local;
			if ( courseTimer != null )
			{
				courseTimer.ResetTimer();
				courseTimer.ResetCheckpoints();
				Respawn();
			}
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Dead ) return;

		var beforeGrounded = Ground != null;
		var beforeVelocity = Velocity;

		BaseVelocity = 0;

		CheckGround();
		CheckPedal();
		var braking = CheckBrake();
		CheckJump();
		DoFriction();
		DoSlope();
		DoTilt();
		DoGroundRotation();

		if ( TimeSincePedalStart < TimeToReachTarget + Time.Delta )
		{
			var a = TimeSincePedalStart / TimeToReachTarget;
			a = Easing.EaseOut( a );

			MovePedals( PedalStartPosition.LerpTo( PedalTargetPosition, a ) );
		}

		DoRotation();
		Gravity();

		if ( Ground == null )
		{
			var hspd = Velocity.WithZ( 0 );
			var maxspd = MaxHorizontalSpeed + 50;
			if ( hspd.Length > maxspd )
			{
				var fraction = maxspd / hspd.Length;
				var prevz = Velocity.z;
				Velocity = (Velocity * fraction).WithZ( prevz );
			}
		}

		Velocity += BaseVelocity;
		Move();
		Velocity -= BaseVelocity;

		if ( Ground != null && !PrevGrounded )
		{
			//AddEvent( "grounded" );
		}

		TimeSincePedalStart += Time.Delta;
		PrevGrounded = beforeGrounded;
		PrevVelocity = beforeVelocity;

		var mapsetting = Scene.GetAllComponents<MapSettings>().FirstOrDefault();
		if ( mapsetting != null )
		{
			mapsetting.AddTimePlayed( 0.01f );
		}
		if ( !Dead && ShouldFall() || ForceFall )
		{
			Fall();

			if ( !IsProxy && mapsetting != null )
			{
				mapsetting.AddFall();
			}
		}
	}

	GameObject Ragdoll;
	public async void Fall()
	{
		Dead = true;

		if ( Ragdoll != null )
		{
			Ragdoll.Destroy();
			Ragdoll = null;
		}

		Ragdoll = new GameObject( true, "Ragdoll" );
		var mrs = GameObject.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		foreach ( var mr in mrs )
		{
			mr.Enabled = false;

			var go = new GameObject( true );
			go.SetParent( Ragdoll );

			go.Transform.Position = mr.Transform.Position;
			go.Transform.Rotation = mr.Transform.Rotation;
			go.Transform.Scale = mr.Transform.Scale;



			var smr = go.Components.Create<SkinnedModelRenderer>();
			smr.Model = mr.Model;
			if ( mr.Tags.Has( "clothing" ))
			{
				Log.Info( "Adding clothing tag" );
				smr.Tags.Add( "clothing" );

				var citizen = Ragdoll.Components.GetAll<SkinnedModelRenderer>().Where( x => x.Tags.Has( "citizen" ) ).FirstOrDefault();
				Log.Info( $"Found citizen: {citizen}" );
				smr.BoneMergeTarget = citizen;
			}
			else
			{ 
				var mphys = go.Components.Create<ModelPhysics>();
				mphys.Model = mr.Model;
				mphys.Renderer = smr;

				mphys.Enabled = true;
				mphys.Enabled = true;
			}
			if ( mr.Tags.Has( "citizen" ) && !mr.Tags.Has( "clothing" ) )
			{
				Log.Info( "Adding citizen tag" );
				smr.Tags.Add( "citizen" );
			}
			smr.MaterialOverride = mr.MaterialOverride;
		}

		if ( FallSound != null )
		{
			Sound.Play( FallSound, Transform.Position );
		}

		await Task.Delay( 3000 );

		Respawn();
	}

	void Respawn()
	{
		if ( Ragdoll != null )
		{
			Ragdoll.Destroy();
			Ragdoll = null;
		}

		var mrs = GameObject.Components.GetAll<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		foreach ( var mr in mrs )
		{
			mr.Enabled = true;
		}

		Dead = false;
		ForceFall = false;

		this.Tilt = default;
		this.JumpTilt = default;
		this.Velocity = default;
		this.BaseVelocity = default;
		this.PrevJumpTilt = default;
		this.PedalTargetPosition = 0;
		this.TimeSincePedalStart = 0;
		this.TimeSinceJumpDown = 0;
		this.TimeSinceNotGrounded = 0;

		var courseTimer = CourseTimer.Local;
		if ( courseTimer != null && courseTimer.TryFindCheckpoint( out var pos, out var fwd ) )
		{
			Transform.Position = pos;
			Transform.Rotation = Rotation.From( fwd );
			TargetForward = fwd;
			CameraController.ViewAngles = fwd;
		}
	}

	private bool ShouldFall()
	{
		if ( NoFall ) return false;

		if ( Ground != null && !PrevGrounded )
		{
			if ( PrevVelocity.z < -1000 )
				return true;
		}

		if ( PrevVelocity.Length > StopSpeed )
		{
			var wallTrStart = Position;
			var wallTrEnd = wallTrStart + PrevVelocity * Time.Delta;
			var tr = TraceBBox( wallTrStart, wallTrEnd, Mins + Vector3.Up * 16, Maxs );

			if ( tr.Hit && Vector3.GetAngle( tr.Normal, Vector3.Up ) > 85f )
			{
				var d = Vector3.Dot( tr.Normal, PrevVelocity );
				if ( d < -.3f )
					return true;
			}
		}

		var ang = Rotation.Angles();
		var aroll = Math.Abs( ang.roll );
		var apitch = Math.Abs( ang.pitch );
		var maxLean = Ground != null ? MaxLean : 180;

		if ( aroll > maxLean || apitch > maxLean )
			return true;

		if ( aroll + apitch > maxLean * 1.50f )
			return true;

		var trs = Scene.Trace.Sphere( 10f, Position + Vector3.Up * 24f, Position + Rotation.Up * 55 )
			.WithoutTags( "player" )
			.Run();

		if ( trs.Hit ) return true;

		return false;
	}

	private void Move()
	{
		var mover = new CharacterControllerHelper();
		mover.Velocity = Velocity;
		mover.Position = Position;
		mover.Trace = Scene.Trace.Size( Mins, Maxs ).WithoutTags( "player" );
		mover.MaxStandableAngle = 75f;
		mover.TryMoveWithStep( Time.Delta, 12 );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	private void CheckGround()
	{
		var tr = TraceBBox( Position, Position + Vector3.Down * 5f, Mins, Maxs, 3f );

		if ( !tr.Hit || Vector3.GetAngle( Vector3.Up, tr.Normal ) < 5f && Velocity.z > 140f )
		{
			if ( Ground != null )
			{
				TimeSinceNotGrounded = 0;
			}
			else
			{
				TimeSinceNotGrounded += Time.Delta;
			}
			Ground = null;
			return;
		}

		if ( Ground == null )
		{
			//AddEvent( "land" );
			Tilt = Rotation.Angles().WithYaw( 0 );
			Position = Position.WithZ( tr.EndPosition.z );
			//new FallCameraModifier( -200f );

			if ( !LengthOf( JumpTilt ).AlmostEqual( 0, .1f ) )
			{
				if ( LengthOf( PrevJumpTilt ) > 35 )
				{
					JumpTilt = PrevJumpTilt * .5f;
					Tilt = PrevJumpTilt * -1f;
				}
				else
				{
					JumpTilt = Angles.Zero;
					Tilt = Angles.Zero;
				}
			}

		}
		//BaseVelocity = tr.Entity.Velocity;
		Ground = tr.GameObject;
		GroundNormal = tr.Normal;
		groundSurface = tr.Surface.ResourceName;
	}

	private void CheckJump()
	{
		if ( Input.Released( "jump" ) && (Ground != null || TimeSinceNotGrounded < .1f) )
		{
			var t = Math.Min( TimeSinceJumpDown / MaxJumpStrengthTime, 1f );
			t = Easing.EaseOut( t );
			var jumpStrength = MinJumpStrength.LerpTo( MaxJumpStrength, t );

			JumpTilt = Tilt * -1;
			PrevJumpTilt = JumpTilt;
			Velocity = Velocity.WithZ( 0 ) + BaseVelocity.WithZ( 0 );
			Velocity += Rotation.Up * jumpStrength;

			if ( GroundNormal.Angle( Vector3.Up ) > 0 )
			{
				Position += Vector3.Up * 5f;
			}

			Ground = null;
			TimeSinceJumpDown = 0;

			//new FallCameraModifier( jumpStrength );

			if ( JumpSound != null )
			{
				Sound.Play( JumpSound, Transform.Position );
			}

			//AddEvent( "jump" );
			return;
		}

		if ( !CanIncrementJump() )
		{
			TimeSinceJumpDown = 0;
			return;
		}

		if ( Input.Down( "jump" ) )
		{
			TimeSinceJumpDown += Time.Delta;
		}
	}

	private bool CanIncrementJump()
	{
		if ( !Input.Down( "jump" ) ) return false;
		if ( Ground != null ) return true;
		if ( TimeSinceNotGrounded < .75f ) return true;

		var tr = TraceBBox( Position, Position + Vector3.Down * 75, Mins, Maxs, 5f );

		if ( !tr.Hit ) return false;

		return true;
	}

	private void CheckPedal()
	{
		if ( !PedalPosition.AlmostEqual( 0f, .1f ) && TimeSincePedalStart > PedalResetAfter )
		{
			SetPedalTarget( 0f, PedalResetTime );
			return;
		}

		if ( Input.Down( "jump" ) ) return;

		//if ( Input.UsingController )
		//{
		//	var ra = Input.GetAnalog( InputAnalog.RightTrigger ).x;
		//	var la = Input.GetAnalog( InputAnalog.LeftTrigger ).x;

		//	if ( ra > 0 && pl.PedalPosition <= .4f && ra > pl.PedalTargetPosition )
		//		SetPedalTarget( ra, PedalTime * ra, InputActions.LeftPedal.Pressed() );

		//	if ( la > 0 && pl.PedalPosition >= -.4f && -la < pl.PedalTargetPosition )
		//		SetPedalTarget( -la, PedalTime * la, InputActions.RightPedal.Pressed() );
		//}
		//else
		{
			if ( Input.Pressed( "left_pedal" ) && PedalPosition >= -.4f )
				SetPedalTarget( -1f, PedalTime, true );

			if ( Input.Pressed( "right_pedal" ) && PedalPosition <= .4f )
				SetPedalTarget( 1f, PedalTime, true );
		}
	}

	private void MovePedals( float newPosition )
	{
		var delta = newPosition - PedalPosition;
		PedalPosition = newPosition;

		// don't add velocity when pedals are returning to idle or in air..
		if ( PedalTargetPosition == 0 ) return;
		if ( Ground == null ) return;

		Tilt += new Angles( 0, 0, 15f * delta );

		var spd = Velocity.WithZ( 0 ).Length;
		if ( spd > MaxHorizontalSpeed ) return;

		var strengthAlpha = Math.Abs( PedalStartPosition );
		var strength = MinPedalStrength.LerpTo( MaxPedalStrength, strengthAlpha );
		var addVelocity = Rotation.Forward * strength * Math.Abs( delta );
		Velocity += addVelocity;

		if ( !Velocity.Length.AlmostEqual( 0 ) && Velocity.Length < StopSpeed )
		{
			Velocity *= StopSpeed / Velocity.Length;
		}
	}

	private bool CheckBrake()
	{
		if ( Ground == null ) return false;
		if ( !Input.Down( "brake" ) ) return false;

		if ( !PrevGrounded && Velocity.WithZ( 0 ).Length < 300 )
		{
			Velocity *= .35f;
		}

		Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * BrakeStrength ).WithZ( Velocity.z );

		return true;
	}

	public float TimeToReachTarget { get; private set; }
	public float PedalStartPosition { get; private set; }
	public float PedalTargetPosition { get; private set; }

	private void SetPedalTarget( float target, float timeToReach, bool tryBoost = false )
	{
		if ( PedalTargetPosition.AlmostEqual( target, .1f ) ) return;

		// check this before moving shit
		var canboost = CanPedalBoost( out bool _, out bool _ );

		if ( PedalSound != null )
		{
			Sound.Play( PedalSound, Transform.Position );
		}

		TimeSincePedalStart = 0;
		TimeToReachTarget = timeToReach;
		PedalStartPosition = PedalPosition;
		PedalTargetPosition = target;

		//new FallCameraModifier( -35f );

		//AddEvent( "pedal" );

		if ( Ground == null ) return;
		if ( !tryBoost || !canboost ) return;

		if ( PerfectPedalSound != null )
		{
			Sound.Play( PerfectPedalSound, Transform.Position );
		}

		PedalAchievement();

		//if ( Pawn.IsLocalPawn )
		//{
		//	new FallCameraModifier( -100f );
		//	Sound.FromEntity( "sounds/unicycle/unicycle.pedal.perfect.sound", pl );
		//}

		Velocity += Rotation.Forward.WithZ( 0 ) * PerfectPedalBoost;
	}

	void PedalAchievement()
	{
		AchievementManager.Instance.UpdateAchievementProgress( "Perfect Pedal Bronze" );
		AchievementManager.Instance.UpdateAchievementProgress( "Perfect Pedal Silver" );
		AchievementManager.Instance.UpdateAchievementProgress( "Perfect Pedal Gold" );
		AchievementManager.Instance.UpdateAchievementProgress( "Perfect Pedal Platinum" );
	}

	private bool NoTilt => false;
	private float PitchAccumulator;
	private void DoTilt()
	{
		if ( NoTilt )
		{
			Tilt = Angles.Zero;
			return;
		}

		// recover tilt from momentum
		var recover = Math.Min( Velocity.WithZ( 0 ).Length / 100f, 1.5f );
		var tilt = Tilt;
		tilt = Angles.Lerp( tilt, Angles.Zero, recover * Time.Delta );

		// tilt from input
		var input = Vector3.Zero;
		if ( Input.Down( "lean_left" ) ) input.z = -1;
		if ( Input.Down( "lean_right" ) ) input.z = 1;
		if ( Input.Down( "lean_forward" ) ) input.x = 1;
		if ( Input.Down( "lean_back" ) ) input.x = -1;

		if ( input.x == 0 )
		{
			PitchAccumulator = 0;
		}

		if ( Ground == null )
		{
			PitchAccumulator += input.x * 2f;
			PitchAccumulator = Math.Clamp( PitchAccumulator, -50, 50 );

			tilt += new Angles( PitchAccumulator / 5f, 0, input.z ) * LeanSpeed * Time.Delta;
		}
		else
		{
			tilt += new Angles( input.x, 0, input.z ) * LeanSpeed * Time.Delta;
		}

		// continue to tip if not in the safe zone, but not when
		// jump tilt is doing its thing or when trying to manually tilt
		var len = LengthOf( tilt );
		if ( len > LeanSafeZone )
		{
			if ( Ground != null && LengthOf( JumpTilt ).AlmostEqual( 0f, .05f ) && input.Length.AlmostEqual( 0 ) )
			{
				var t = len / MaxLean;
				var str = .25f.LerpTo( TipSpeed, t );
				tilt += tilt * str * Time.Delta;
			}
		}

		// tilt from changes in velocity
		if ( Ground != null )
		{
			var velDiff = Transform.Local.NormalToLocal( Velocity - PrevVelocity );

			// cancel out a bit of the pitch if we're turning sharp
			if ( Math.Abs( velDiff.y ) > Math.Abs( velDiff.x ) ) velDiff.x *= .25f;

			tilt += new Angles( -velDiff.x * ForwardVelocityTilt, 0, -velDiff.y * RightVelocityTilt ) * Time.Delta;
		}

		// tilt while on uneven ground
		var groundAngle = Vector3.GetAngle( GroundNormal, Vector3.Up );
		if ( Ground != null && groundAngle > 3f && groundAngle < 75f )
		{
			var groundRot = FromToRotation( Vector3.Up, !NoTilt ? GroundNormal : Vector3.Up );
			groundRot *= TargetForward;
			var groundTilt = groundRot.Angles().WithYaw( 0 );
			var tiltAlpha = Easing.EaseIn( groundAngle / 30f );

			tilt += groundTilt * tiltAlpha * SlopeTipSpeed * Time.Delta;
		}

		// this handles how we tilt and recover tilt after jumping
		tilt += JumpTilt.Normal * 3f * Time.Delta;

		if ( Ground != null )
		{
			var before = JumpTilt;
			JumpTilt = Angles.Lerp( JumpTilt, Angles.Zero, 3f * Time.Delta );
			var delta = before - JumpTilt;
			tilt += delta;
		}

		tilt.roll = Math.Clamp( tilt.roll, -MaxLean - 5, MaxLean + 5 );
		//tilt.pitch = Math.Clamp( tilt.pitch, -MaxLean - 5, MaxLean + 5 );

		Tilt = tilt;
	}

	private void DoRotation()
	{
		var spd = Velocity.WithZ( 0 ).Length;
		var grounded = Ground != null;
		var inputFwd = CameraController.ViewAngles.ToRotation().Forward.WithZ( 0 );

		var canTurn = (!grounded && spd < MaxAirTurnSpeed) || (grounded && spd > StopSpeed);

		if ( canTurn )
		{
			var inputRot = FromToRotation( Vector3.Forward, inputFwd );
			var turnSpeed = grounded ? GroundTurnSpeed : AirTurnSpeed;
			TargetForward = Rotation.Slerp( TargetForward, inputRot, turnSpeed * Time.Delta );

			if ( grounded )
			{
				var n = Vector3.Cross( TargetForward.Forward, GroundNormal ).Normal;
				Velocity = ClipVelocity( Velocity, n );
			}
		}

		//var targetRot = FromToRotation( Vector3.Up, !NoTilt ? GroundNormal : Vector3.Up );
		var targetRot = TargetForward;
		targetRot *= Rotation.From( Tilt );
		Rotation = Rotation.Slerp( Rotation, targetRot, 6.5f * Time.Delta );

		// zero velocity if on flat ground and moving slow
		if ( grounded && Velocity.Length < StopSpeed && Vector3.GetAngle( Vector3.Up, GroundNormal ) < 5f )
		{
			Velocity = 0;
		}
	}

	private void Gravity()
	{
		if ( Ground != null )
		{
			if ( Vector3.GetAngle( GroundNormal, Vector3.Up ) <= 5 )
			{
				Velocity = Velocity.WithZ( 0 );
			}
			return;
		}

		Velocity -= new Vector3( 0, 0, 800f * Time.Delta );
	}

	private Rotation prevRot;
	private GameObject prevGroundEntity;
	private void DoGroundRotation()
	{
		if ( Ground == null ) return;
		if ( prevRot == Ground.Transform.Rotation ) return;

		if ( prevGroundEntity == Ground )
		{
			var delta = Rotation.Difference( prevRot, Ground.Transform.Rotation );
			Position = RotateAroundPivot( Position, Ground.Transform.Position, delta );
		}

		prevGroundEntity = Ground;
		prevRot = Ground.Transform.Rotation;
	}

	public bool CanPedalBoost( out bool leftPedal, out bool rightPedal )
	{
		leftPedal = false;
		rightPedal = false;

		var time = TimeSincePedalStart - PedalTime;
		var canboost = time < PerfectPedalTimeframe && time > 0f;

		if ( canboost )
		{
			leftPedal = PedalPosition > .75f;
			rightPedal = PedalPosition < -.75f;
		}

		var spd = Velocity.WithZ( 0 ).Length;

		return canboost && spd < MaxHorizontalSpeed;
	}

	float SurfaceFriction => 1.0f;
	private void DoFriction()
	{
		if ( Ground == null ) return;

		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		var drop = speed * Time.Delta * .1f * GetSurfaceFriction();
		var newspeed = Math.Max( speed - drop, 0 );

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}
	private float GetSurfaceFriction()
	{
		return groundSurface switch
		{
			"mud" => 5.0f,
			"sand" => 20.0f,
			"dirt" => 2.0f,
			_ => 1.0f,
		};
	}

	private void DoSlope()
	{
		if ( Ground == null ) return;
		if ( Input.Down( "brake" ) && Velocity.Length < StopSpeed * 4 ) return;

		var slopeAngle = Vector3.GetAngle( GroundNormal, Vector3.Up );
		if ( slopeAngle == 0 ) return;

		var heading = Vector3.Dot( GroundNormal, Rotation.Forward.WithZ( 0 ).Normal );
		var dir = Vector3.Cross( GroundNormal, Rotation.Right ).Normal;

		var left = Vector3.Cross( GroundNormal, Vector3.Up ).Normal;
		var slopeDir = Vector3.Cross( GroundNormal, left ).Normal;
		var strengthRatio = slopeAngle / 90f;
		var strength = SlopeSpeed * strengthRatio * Math.Abs( Vector3.Dot( dir, slopeDir ) );
		var velocityVector = dir * strength * Math.Sign( heading );

		Velocity += velocityVector * Time.Delta;

		if ( Vector3.Dot( Velocity.Normal, slopeDir ) < 0 )
		{
			Velocity = ClipVelocity( Velocity, GroundNormal );
		}
	}

	public SceneTraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		const float TraceOffset = 0f;

		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Scene.Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithoutTags( "player" )
					.Run();

		tr.EndPosition -= TraceOffset;
		return tr;
	}

	static Rotation FromToRotation( Vector3 aFrom, Vector3 aTo )
	{
		Vector3 axis = Vector3.Cross( aFrom, aTo );
		float angle = Vector3.GetAngle( aFrom, aTo );
		return AngleAxis( angle, axis.Normal );
	}

	static Rotation AngleAxis( float aAngle, Vector3 aAxis )
	{
		aAxis = aAxis.Normal;
		float rad = aAngle * MathX.DegreeToRadian( .5f );
		aAxis *= MathF.Sin( rad );
		return new Quaternion( aAxis.x, aAxis.y, aAxis.z, MathF.Cos( rad ) );
	}

	static Vector3 ClipVelocity( Vector3 vel, Vector3 norm, float overbounce = 1.0f )
	{
		var backoff = Vector3.Dot( vel, norm ) * overbounce;
		var o = vel - (norm * backoff);

		// garry: I don't totally understand how we could still
		//		  be travelling towards the norm, but the hl2 code
		//		  does another check here, so we're going to too.
		var adjust = Vector3.Dot( o, norm );
		if ( adjust >= 1.0f ) return o;

		adjust = MathF.Min( adjust, -1.0f );
		o -= norm * adjust;

		return o;
	}

	public static Vector3 RotateAroundPivot( Vector3 pos, Vector3 pivot, Rotation rot )
	{
		return rot * (pos - pivot) + pivot;
	}

	static float LengthOf( Angles angle )
	{
		var vec = new Vector3( angle.pitch, angle.yaw, angle.roll );
		return vec.Length;
	}

}
