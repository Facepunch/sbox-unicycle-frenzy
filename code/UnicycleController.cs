﻿using System;
using System.Numerics;
using Sandbox;
using Sandbox.ScreenShake;

internal partial class UnicycleController : BasePlayerController
{

	[Net, Predicted]
	public float PedalPosition { get; set; } // -1 = left pedal down, 1 = right pedal down, 0 = flat
	[Net, Predicted]
	public Angles Lean { get; set; }
	[Net]
	public float PedalTime { get; set; } = .75f;
	[Net]
	public float PedalResetAfter { get; set; } = 1.5f;
	[Net]
	public float PedalResetTime { get; set; } = .75f;
	[Net]
	public float MinPedalStrength { get; set; } = 10f;
	[Net]
	public float MaxPedalStrength { get; set; } = 50f;
	[Net]
	public float JumpStrength { get; set; } = 300f;
	[Net]
	public float PerfectPedalMultiplier { get; set; } = 1.25f;


	private Unstuck unstuck;
	private float timeToReachTarget;
	private float pedalStartPosition;
	private float pedalTargetPosition;
	private TimeSince timeSincePedalStart;

	public UnicycleController()
	{
		unstuck = new( this );
	}

	public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRot = Rotation.Identity;
	}

	public override void Simulate()
	{
		base.Simulate();

		if ( Pawn is not UnicyclePlayer pl ) return;

		if ( unstuck.TestAndFix() ) return;

		SetTag( "sitting" );
		TryPedal();
		Move();
		Gravity();
		CheckGround();
		CheckJump();
		Friction();

		// todo:	gradually lean in direction we're already leaning
		//			roll w/ ground normal

		// lerp pedals into place, adding velocity and lean
		if ( timeSincePedalStart < timeToReachTarget + Time.Delta )
		{
			var a = timeSincePedalStart / timeToReachTarget;
			a = Easing.EaseOut( a );

			var newPosition = pedalStartPosition.LerpTo( pedalTargetPosition, a );
			var delta = newPosition - PedalPosition;

			MovePedals( delta );
		}

		// momentum helps keep us straight
		var recover = Velocity.WithZ( 0 ).Length / 200f;
		Lean = Angles.Lerp( Lean, Angles.Zero, recover * Time.Delta );
		Lean += new Angles( Input.Forward, 0, -Input.Left ) * Time.Delta * 40;

		// ground normal -> forward -> Lean
		var targetRot = FromToRotation( Vector3.Up, GroundNormal );
		targetRot *= Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );
		targetRot *= Rotation.From( Lean );

		Rotation = Rotation.Slerp( Rotation, targetRot, Time.Delta * 5f );

		if ( pl.IsServer && ShouldFall() )
		{
			// check fall in player?
			pl.Fall();
			return;
		}

		if ( GroundEntity != null )
		{
			var spd = Velocity.WithZ( 0 ).Length;
			var dir = Rotation.Forward;
			Velocity = dir * spd;
		}
	}

	private bool ShouldFall()
	{
		var spd = Velocity.WithZ( 0 ).Length;

		if ( GroundEntity != null && spd > 5 )
		{
			var d = MathF.Abs( Vector3.Dot( Rotation.Forward, Velocity.WithZ( 0 ).Normal ) );
			if ( d < .8f )
				return true;
		}

		var ang = Rotation.Angles();
		var aroll = Math.Abs( ang.roll );
		var apitch = Math.Abs( ang.pitch );

		if ( aroll > 30 || apitch > 30 )
			return true;

		if ( aroll + apitch > 45 )
			return true;

		return false;
	}

	private void Move()
	{
		var mins = new Vector3( -15, -15, 0 );
		var maxs = new Vector3( +15, +15, 48 );
		var mover = new MoveHelper( Position, Velocity );
		mover.Trace = mover.Trace.Size( mins, maxs ).Ignore( Pawn );
		mover.MaxStandableAngle = 46.0f;
		mover.WallBounce = 0f;
		mover.TryMoveWithStep( Time.Delta, 16 );

		Position = mover.Position;
		Velocity = mover.Velocity;
	}

	private void CheckGround()
	{
		var tr = TraceBBox( Position, Position + Vector3.Down * 11f );

		if ( !tr.Hit )
		{
			GroundEntity = null;
			return;
		}

		Velocity = Velocity.WithZ( 0 );
		GroundEntity = tr.Entity;
		GroundNormal = tr.Normal;
		Position = Position.WithZ( tr.EndPos.z + 10 );
	}

	private void CheckJump()
	{
		if ( GroundEntity == null ) return;
		if ( !Input.Pressed( InputButton.Jump ) ) return;

		Velocity += new Vector3( 0, 0, JumpStrength );
		GroundEntity = null;
	}

	private void Gravity()
	{
		if ( GroundEntity != null )
		{
			Velocity = Velocity.WithZ( 0 );
			return;
		}

		Velocity -= new Vector3( 0, 0, 800f * Time.Delta );
	}

	private void Friction()
	{
		// temp shit
		if ( GroundEntity != null )
		{
			Velocity *= .99f;
		}
	}

	private void TryPedal()
	{
		if ( !PedalPosition.AlmostEqual( 0f, .1f ) && timeSincePedalStart > PedalResetAfter )
		{
			SetPedalTarget( 0f, PedalResetTime );
			return;
		}

		if ( Input.Pressed( InputButton.Prev ) && PedalPosition >= -.4f )
			SetPedalTarget( -1f, PedalTime, true );

		if ( Input.Pressed( InputButton.Use ) && PedalPosition <= .4f )
			SetPedalTarget( 1f, PedalTime, true );
	}

	private void SetPedalTarget( float target, float timeToReach, bool tryBoost = false )
	{
		var prevStart = PedalPosition;
		var prevStartTime = timeSincePedalStart;

		timeSincePedalStart = 0;
		timeToReachTarget = timeToReach;
		pedalStartPosition = PedalPosition;
		pedalTargetPosition = target;

		if ( !tryBoost ) return;
		if ( GroundEntity == null ) return;
		if ( Math.Abs( prevStart ) <= .95f ) return;
		if ( prevStartTime > PedalTime ) return;

		var spd = Velocity.WithZ( 0 ).Length;
		spd *= PerfectPedalMultiplier;

		if( Pawn.IsLocalPawn )
		{
			new Perlin();
			// sound + particle
		}

		Velocity = Velocity.Normal * spd;
	}

	private void MovePedals( float delta )
	{
		PedalPosition += delta;

		// don't add velocity when pedals are returning to idle or in air..
		if ( pedalTargetPosition == 0 ) return;
		if ( GroundEntity == null ) return;

		var strengthAlpha = Math.Abs( pedalStartPosition );
		var strength = MinPedalStrength.LerpTo( MaxPedalStrength, strengthAlpha );

		Lean += new Angles( 0, 0, 30f * Math.Sign( delta ) * Time.Delta );
		Velocity += Rotation.Forward * strength * Math.Abs( delta );
	}

	public static Rotation FromToRotation( Vector3 aFrom, Vector3 aTo )
	{
		Vector3 axis = Vector3.Cross( aFrom, aTo );
		float angle = Vector3.GetAngle( aFrom, aTo );
		return AngleAxis( angle, axis.Normal );
	}

	public static Rotation AngleAxis( float aAngle, Vector3 aAxis )
	{
		aAxis = aAxis.Normal;
		float rad = aAngle * MathX.DegreeToRadian( .5f );
		aAxis *= MathF.Sin( rad );
		return new Quaternion( aAxis.x, aAxis.y, aAxis.z, MathF.Cos( rad ) );
	}

}
