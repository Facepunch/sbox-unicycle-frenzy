
using System;
using System.Numerics;
using Sandbox;

internal partial class UnicycleController : BasePlayerController
{

	[ConVar.Replicated( "uf_debug_nofall" )]
	public static bool NoFall { get; set; } = false;

	[ConVar.Replicated( "uf_debug_notilt" )]
	public static bool NoTilt { get; set; } = false;

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
	public float BrakeStrength => 4.5f;
	public float StopSpeed => 10f;
	public float MaxAirTurnSpeed => 35f;
	public float ForwardVelocityTilt => 3f;
	public float RightVelocityTilt => 1.5f;
	public int MaxHorizontalSpeed => 800;

	private UnicyclePlayer pl => Pawn as UnicyclePlayer;
	public Vector3 Mins => new( -1, -1, 0 );
	public Vector3 Maxs => new( 1, 1, 16 );

	private UnicycleUnstuck unstuck;
	private bool wasBraking;

	public UnicycleController()
	{
		unstuck = new( this );
	}

	public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRotation = Rotation.Identity;

		if ( Debug )
		{
			var intpos = new Vector3( (int)Position.x, (int)Position.y, (int)Position.z );
			DebugOverlay.Text( "Speed: " + Velocity.Length, Position );
			DebugOverlay.Text( "Position: " + intpos, Position + Vector3.Down * 3 );
			DebugOverlay.Text( "Grounded: " + (GroundEntity != null), Position + Vector3.Down * 6 );
			DebugOverlay.Text( "GroundNormal: " + GroundNormal, Position + Vector3.Down * 9 );
			DebugOverlay.Text( "Surface: " + pl.SurfaceFriction, Position + Vector3.Down * 12 );
			DebugOverlay.Text( "Water Level: " + Pawn.WaterLevel, Position + Vector3.Down * 15 );
			DebugOverlay.Text( "Tilt: " + pl.Tilt, Position + Vector3.Down * 18 );

			DebugOverlay.Line( Position, Position + Velocity, Color.Yellow );
		}
	}

	public override void Simulate()
	{
		if ( pl == null || pl.Fallen ) return;
		if ( unstuck.TestAndFix() ) return;

		var beforeGrounded = GroundEntity != null;
		var beforeVelocity = Velocity;

		BaseVelocity = Vector3.Zero;

		CheckGround();
		CheckPedal();
		var braking = CheckBrake();
		CheckJump();
		DoFriction();
		DoSlope();
		DoTilt();
		DoGroundRotation();
		DoBrakeTrail( braking, wasBraking );
		wasBraking = braking;

		// lerp pedals into place, adding velocity and lean
		if ( pl.TimeSincePedalStart < pl.TimeToReachTarget + Time.Delta )
		{
			var a = pl.TimeSincePedalStart / pl.TimeToReachTarget;
			a = Easing.EaseOut( a );

			MovePedals( pl.PedalStartPosition.LerpTo( pl.PedalTargetPosition, a ) );
		}

		DoRotation();
		Gravity();

		if( GroundEntity == null )
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

		// go
		Velocity += BaseVelocity;
		Move();
		Velocity -= BaseVelocity;

		if( GroundEntity != null && !pl.PrevGrounded )
		{
			AddEvent( "grounded" );
		}

		pl.TimeSincePedalStart += Time.Delta;
		pl.PrevGrounded = beforeGrounded;
		pl.PrevVelocity = beforeVelocity;

		if ( ShouldFall() )
		{
			pl.Fall();

			AddEvent( "fall" );
		}
	}

	private Rotation prevRot;
	private Entity prevGroundEntity;
	private void DoGroundRotation()
	{
		if ( GroundEntity == null ) return;
		if ( prevRot == GroundEntity.Rotation ) return;

		if ( prevGroundEntity == GroundEntity )
		{
			var delta = Rotation.Difference( prevRot, GroundEntity.Rotation );
			Position = Position.RotateAroundPivot( GroundEntity.Position, delta );
		}

		prevGroundEntity = GroundEntity;
		prevRot = GroundEntity.Rotation;
	}

	private bool ShouldFall()
	{
		if ( NoFall ) return false;

		if ( GroundEntity != null && !pl.PrevGrounded )
		{
			if ( pl.PrevVelocity.z < -1000 )
				return true;
		}

		if ( pl.PrevVelocity.Length > StopSpeed )
		{
			var wallTrStart = Position;
			var wallTrEnd = wallTrStart + pl.PrevVelocity * Time.Delta;
			var tr = TraceBBox( wallTrStart, wallTrEnd, Mins + Vector3.Up * 16, Maxs );

			if ( tr.Hit && Vector3.GetAngle( tr.Normal, Vector3.Up ) > 85f )
			{
				var d = Vector3.Dot( tr.Normal, pl.PrevVelocity );
				if ( d < -.3f )
					return true;
			}
		}

		var ang = pl.Rotation.Angles();
		var aroll = Math.Abs( ang.roll );
		var apitch = Math.Abs( ang.pitch );
		var maxLean = GroundEntity != null ? MaxLean : 180;

		if ( aroll > maxLean || apitch > maxLean )
			return true;

		if ( aroll + apitch > maxLean * 1.50f )
			return true;

		var trs = Trace.Sphere( 10f, Position + Vector3.Up * 24f, Position + Rotation.Up * 55 )
			.Ignore( Pawn )
			.WithoutTags( "player" )
			.Run();

		if( trs.Hit ) return true;

		return false;
	}

	private void Move()
	{
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( Mins, Maxs ).WithoutTags( "player" ).Ignore( Pawn );
		mover.MaxStandableAngle = 75f;
		mover.TryMoveWithStep( Time.Delta, 12 );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	private void DoSlope()
	{
		if ( GroundEntity == null ) return;
		if ( InputActions.Brake.Down() && Velocity.Length < StopSpeed * 4 ) return;

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

		if( Vector3.Dot( Velocity.Normal, slopeDir ) < 0 )
		{
			Velocity = ClipVelocity( Velocity, GroundNormal );
		}

		if ( Debug )
		{
			DebugOverlay.Line( Position, Position + velocityVector, Color.Red );
		}
	}

	private void CheckGround()
	{
		var tr = TraceBBox( Position, Position + Vector3.Down * 4f, Mins, Maxs, 3f );

		if ( !tr.Hit || Vector3.GetAngle( Vector3.Up, tr.Normal ) < 5f && Velocity.z > 140f )
		{
			if ( GroundEntity != null )
			{
				pl.TimeSinceNotGrounded = 0;
			}
			else
			{
				pl.TimeSinceNotGrounded += Time.Delta;
			}
			GroundEntity = null;
			return;
		}

		if ( GroundEntity == null )
		{
			AddEvent( "land" );
			pl.Tilt = Rotation.Angles().WithYaw( 0 );
			Position = Position.WithZ( tr.EndPosition.z );
			new FallCameraModifier( -200f );

			if ( !LengthOf( pl.JumpTilt ).AlmostEqual( 0, .1f ) )
			{
				if ( LengthOf( pl.PrevJumpTilt ) > 35 )
				{
					pl.JumpTilt = pl.PrevJumpTilt * .5f;
					pl.Tilt = pl.PrevJumpTilt * -1f;
				}
				else
				{
					pl.JumpTilt = Angles.Zero;
					pl.Tilt = Angles.Zero;
				}
			}

		}

		BaseVelocity = tr.Entity.Velocity; 
		GroundEntity = tr.Entity;
		GroundNormal = tr.Normal;
	}

	private void Gravity()
	{
		if ( GroundEntity != null )
		{
			if ( Vector3.GetAngle( GroundNormal, Vector3.Up ) <= 5 )
			{
				Velocity = Velocity.WithZ( 0 );
			}
			return;
		}

		Velocity -= new Vector3( 0, 0, 800f * Time.Delta );
	}

	private void DoFriction()
	{
		if ( GroundEntity == null ) return;

		var speed = Velocity.Length;
		if ( speed < 0.1f ) return;

		var drop = speed * Time.Delta * .1f * pl.SurfaceFriction;
		var newspeed = Math.Max( speed - drop, 0 );

		if ( newspeed != speed )
		{
			newspeed /= speed;
			Velocity *= newspeed;
		}
	}

	private float PitchAccumulator;
	private void DoTilt()
	{
		if ( NoTilt )
		{
			pl.Tilt = Angles.Zero;
			return;
		}

		// recover tilt from momentum
		var recover = Math.Min( Velocity.WithZ( 0 ).Length / 100f, 1.5f );
		var tilt = pl.Tilt;
		tilt = Angles.Lerp( tilt, Angles.Zero, recover * Time.Delta );

		// tilt from input
		var input = new Vector3( Input.Forward, 0, -Input.Left );

		var pitchmult = GroundEntity == null ? 1.5f : .5f;
		PitchAccumulator += input.x * pitchmult;
		PitchAccumulator = Math.Clamp( PitchAccumulator, -50, 50 );

		if ( input.x == 0 )
		{
			PitchAccumulator = 0;
		}

		tilt += new Angles( PitchAccumulator / 5f, 0, input.z ) * LeanSpeed * Time.Delta;

		// continue to tip if not in the safe zone, but not when
		// jump tilt is doing its thing or when trying to manually tilt
		var len = LengthOf( tilt );
		if( len > LeanSafeZone )
		{
			if ( GroundEntity != null && LengthOf( pl.JumpTilt ).AlmostEqual( 0f, .05f ) && input.Length.AlmostEqual( 0 ) )
			{
				var t = len / MaxLean;
				var str = .25f.LerpTo( TipSpeed, t );
				tilt += tilt * str * Time.Delta;
			}
		}

		// tilt from changes in velocity
		if ( GroundEntity != null )
		{
			var velDiff = pl.Transform.NormalToLocal( Velocity - pl.PrevVelocity );

			// cancel out a bit of the pitch if we're turning sharp
			if ( Math.Abs( velDiff.y ) > Math.Abs( velDiff.x ) ) velDiff.x *= .25f;

			tilt += new Angles( -velDiff.x * ForwardVelocityTilt, 0, -velDiff.y * RightVelocityTilt ) * Time.Delta;
		}

		// tilt while on uneven ground
		var groundAngle = Vector3.GetAngle( GroundNormal, Vector3.Up );
		if ( GroundEntity != null && groundAngle > 3f && groundAngle < 75f )
		{
			var groundRot = FromToRotation( Vector3.Up, !NoTilt ? GroundNormal : Vector3.Up );
			groundRot *= pl.TargetForward;
			var groundTilt = groundRot.Angles().WithYaw( 0 );
			var tiltAlpha = Easing.EaseIn( groundAngle / 30f );

			tilt += groundTilt * tiltAlpha * SlopeTipSpeed * Time.Delta;
		}

		// this handles how we tilt and recover tilt after jumping
		tilt += pl.JumpTilt.Normal * 3f * Time.Delta;

		if ( GroundEntity != null )
		{
			var before = pl.JumpTilt;
			pl.JumpTilt = Angles.Lerp( pl.JumpTilt, Angles.Zero, 3f * Time.Delta );
			var delta = before - pl.JumpTilt;
			tilt += delta;
		}

		if ( IsIdle() )
		{
			// Jake: I'm using t in a silly way here
			//		 I don't want it to lerp from A to B by T because
			//		 we can't really snap the tilt like that without
			//		 conflicting with the player's input, so lerp w/
			//		 delta and use t for easing
			var rndTilt = GetRandomTilt( out float t );
			tilt = Angles.Lerp( tilt, rndTilt, Time.Delta * t );
		}

		tilt.roll = Math.Clamp( tilt.roll, -MaxLean - 5, MaxLean + 5 );
		//tilt.pitch = Math.Clamp( tilt.pitch, -MaxLean - 5, MaxLean + 5 );

		pl.Tilt = tilt;
	}

	private bool IsIdle()
	{
		var input = new Vector3( Input.Forward, 0, -Input.Left );

		if ( GroundEntity == null ) return false;
		if ( !input.Length.AlmostEqual( 0f ) ) return false;
		if ( !LengthOf( pl.JumpTilt ).AlmostEqual( 0f ) ) return false;
		if ( pl.TimeSincePedalStart < PedalTime ) return false;
		if ( LengthOf( pl.Tilt ) >= LeanSafeZone ) return false;
		if ( InputActions.Brake.Down() && Velocity.Length > StopSpeed ) return false;

		return true;
	}

	private Angles GetRandomTilt( out float t )
	{
		var seed = pl.NetworkIdent + ( Time.Tick / 50f );
		t = seed - (int)seed;

		Rand.SetSeed( (int)seed );
		var tilt = Angles.Random.WithYaw( 0 );
		tilt.roll *= 2f;

		return tilt.Normal * LeanSafeZone * .75f;
	}

	private void DoRotation()
	{
		var spd = Velocity.WithZ( 0 ).Length;
		var grounded = GroundEntity != null;
		var inputFwd = Input.Rotation.Forward.WithZ( 0 );

		var canTurn = (!grounded && spd < MaxAirTurnSpeed) || (grounded && spd > StopSpeed);

		if ( canTurn )
		{
			var inputRot = FromToRotation( Vector3.Forward, inputFwd );
			var turnSpeed = grounded ? GroundTurnSpeed : AirTurnSpeed;
			pl.TargetForward = Rotation.Slerp( pl.TargetForward, inputRot, turnSpeed * Time.Delta );

			if( grounded )
			{
				var n = Vector3.Cross( pl.TargetForward.Forward, GroundNormal ).Normal;
				Velocity = ClipVelocity( Velocity, n );
			}
		}

		//var targetRot = FromToRotation( Vector3.Up, !NoTilt ? GroundNormal : Vector3.Up );
		var targetRot = pl.TargetForward;
		targetRot *= Rotation.From( pl.Tilt );
		Rotation = Rotation.Slerp( Rotation, targetRot, 6.5f * Time.Delta );

		// zero velocity if on flat ground and moving slow
		if ( grounded && Velocity.Length < StopSpeed && Vector3.GetAngle( Vector3.Up, GroundNormal ) < 5f )
		{
			Velocity = 0;
		}
	}

	private void CheckJump()
	{
		if ( InputActions.Jump.Released() && (GroundEntity != null || pl.TimeSinceNotGrounded < .1f) )
		{
			var t = Math.Min( pl.TimeSinceJumpDown / MaxJumpStrengthTime, 1f );
			t = Easing.EaseOut( t );
			var jumpStrength = MinJumpStrength.LerpTo( MaxJumpStrength, t );

			pl.JumpTilt = pl.Tilt * -1;
			pl.PrevJumpTilt = pl.JumpTilt;
			Velocity = Velocity.WithZ( 0 ) + BaseVelocity.WithZ( 0 );
			Velocity += Rotation.Up * jumpStrength;

			if ( GroundNormal.Angle( Vector3.Up ) > 0 )
			{
				Position += Vector3.Up * 5f;
			}

			GroundEntity = null;
			pl.TimeSinceJumpDown = 0;

			new FallCameraModifier( jumpStrength );

			Sound.FromEntity( "sounds/unicycle/unicycle.jump.sound", pl );

			AddEvent( "jump" );
			return;
		}

		if ( !CanIncrementJump() )
		{
			pl.TimeSinceJumpDown = 0;
			return;
		}

		if ( InputActions.Jump.Down() )
		{
			pl.TimeSinceJumpDown += Time.Delta;
		}
	}

	private bool CanIncrementJump()
	{
		if ( !InputActions.Jump.Down() ) return false;
		if ( GroundEntity != null ) return true;
		if ( pl.TimeSinceNotGrounded < .75f ) return true;

		var tr = TraceBBox( Position, Position + Vector3.Down * 75, 5f );

		if ( !tr.Hit ) return false;

		return true;
	}

	private void CheckPedal()
	{
		if ( !pl.PedalPosition.AlmostEqual( 0f, .1f ) && pl.TimeSincePedalStart > PedalResetAfter )
		{
			SetPedalTarget( 0f, PedalResetTime );
			return;
		}

		if ( InputActions.Jump.Down() ) return;

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
			if ( InputActions.LeftPedal.Pressed() && pl.PedalPosition >= -.4f )
				SetPedalTarget( -1f, PedalTime, true );

			if ( InputActions.RightPedal.Pressed() && pl.PedalPosition <= .4f )
				SetPedalTarget( 1f, PedalTime, true );
		}
	}

	private bool CheckBrake()
	{
		if ( GroundEntity == null ) return false;
		if ( !InputActions.Brake.Down() ) return false;

		if ( !pl.PrevGrounded && Velocity.WithZ( 0 ).Length < 300 )
		{
			Velocity *= .35f;
		}

		Velocity = Velocity.LerpTo( Vector3.Zero, Time.Delta * BrakeStrength ).WithZ( Velocity.z );

		return true;
	}

	private BrakeTrail BrakeTrail;
	private void DoBrakeTrail( bool brakingNow, bool wasBraking )
	{
		if ( brakingNow == wasBraking ) return;
		if ( Host.IsClient ) return;

		if( brakingNow )
		{
			BrakeTrail = new();
			BrakeTrail.SetParent( Pawn );
		}
		else
		{
			BrakeTrail?.SetParent( null );
		}
	}

	public bool CanPedalBoost( out bool leftPedal, out bool rightPedal )
	{
		leftPedal = false;
		rightPedal = false;

		if ( Pawn is not UnicyclePlayer player ) 
			return false;

		var time = player.TimeSincePedalStart - PedalTime;
		var canboost = time < PerfectPedalTimeframe && time > 0f;

		if ( canboost )
		{
			leftPedal = player.PedalPosition > .75f;
			rightPedal = player.PedalPosition < -.75f;
		}

		var spd = Velocity.WithZ( 0 ).Length;

		return canboost && spd < MaxHorizontalSpeed;
	}

	private void SetPedalTarget( float target, float timeToReach, bool tryBoost = false )
	{
		if ( pl.PedalTargetPosition.AlmostEqual( target, .1f ) ) return;

		// check this before moving shit
		var canboost = CanPedalBoost( out bool _, out bool _ );

		pl.TimeSincePedalStart = 0;
		pl.TimeToReachTarget = timeToReach;
		pl.PedalStartPosition = pl.PedalPosition;
		pl.PedalTargetPosition = target;

		new FallCameraModifier( -35f );

		AddEvent( "pedal" );

		if ( GroundEntity == null ) return;
		if ( !tryBoost || !canboost ) return;

		if ( Pawn.IsLocalPawn )
		{
			new FallCameraModifier( -100f );
			Sound.FromEntity( "sounds/unicycle/unicycle.pedal.perfect.sound", pl );
		}

		Velocity += Rotation.Forward.WithZ( 0 ) * PerfectPedalBoost;
	}

	private void MovePedals( float newPosition )
	{
		var delta = newPosition - pl.PedalPosition;
		pl.PedalPosition = newPosition;

		// don't add velocity when pedals are returning to idle or in air..
		if ( pl.PedalTargetPosition == 0 ) return;
		if ( GroundEntity == null ) return;

		pl.Tilt += new Angles( 0, 0, 15f * delta );

		var spd = Velocity.WithZ( 0 ).Length;
		if ( spd > MaxHorizontalSpeed ) return;

		var strengthAlpha = Math.Abs( pl.PedalStartPosition );
		var strength = MinPedalStrength.LerpTo( MaxPedalStrength, strengthAlpha );
		var addVelocity = Rotation.Forward * strength * Math.Abs( delta );
		Velocity += addVelocity;

		if ( !Velocity.Length.AlmostEqual( 0 ) && Velocity.Length < StopSpeed )
		{
			Velocity *= StopSpeed / Velocity.Length;
		}
	}

	public override TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start + TraceOffset, end + TraceOffset )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( Pawn )
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

	static float LengthOf( Angles angle )
	{
		var vec = new Vector3( angle.pitch, angle.yaw, angle.roll );
		return vec.Length;
	}

}
