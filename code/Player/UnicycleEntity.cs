
using Sandbox;
using System;

internal partial class UnicycleEntity : Entity
{

	[Net]
	public ModelEntity Frame { get; set; }
	[Net]
	public ModelEntity Seat { get; set; }
	[Net]
	public ModelEntity Wheel { get; set; }
	[Net]
	public ModelEntity LeftPedal { get; set; }
	[Net]
	public ModelEntity RightPedal { get; set; }
	[Net]
	public Entity Pedals { get; set; }
	[Net]
	public Entity WheelPivot { get; set; }

	private Particles trailParticle;
	private Entity localPawnPedals;
	private ModelEntity localLeftPedal;
	private ModelEntity localRightPedal;

	public ModelEntity DisplayedLeftPedal => localLeftPedal ?? LeftPedal;
	public ModelEntity DisplayedRightPedal => localRightPedal ?? RightPedal;

	public Vector3 GetAssPosition()
	{
		var assAttachment = Seat.GetAttachment( "Ass" );
		if ( !assAttachment.HasValue )
		{
			return Vector3.Zero;
		}

		return assAttachment.Value.Position;
	}

	private void AssembleParts()
	{
		Host.AssertServer();

		if ( Parent is not UnicyclePlayer pl ) return;

		Frame?.Delete();
		Frame = null;
		trailParticle?.Destroy();
		trailParticle = null;

		var cfg = pl.Client.Components.Get<CustomizationComponent>();

		var frame = cfg.GetEquippedPart( PartType.Frame );
		var seat = cfg.GetEquippedPart( PartType.Seat );
		var wheel = cfg.GetEquippedPart( PartType.Wheel );
		var pedal = cfg.GetEquippedPart( PartType.Pedal );
		var trail = cfg.GetEquippedPart( PartType.Trail );

		Frame = new ModelEntity( frame.Model );
		Frame.SetParent( this, null, Transform.Zero );

		Seat = new ModelEntity( seat.Model );
		Seat.SetParent( Frame, "seat", Transform.Zero );

		WheelPivot = new Entity();
		WheelPivot.SetParent( Frame, "hub", Transform.Zero );

		Wheel = new ModelEntity( wheel.Model );
		Wheel.SetParent( WheelPivot, null, Transform.Zero );

		var wheelRadius = Wheel.GetAttachment( "hud", false )?.Position.z ?? 12f;

		Wheel.LocalPosition -= Vector3.Up * wheelRadius;
		Frame.LocalPosition = Vector3.Up * wheelRadius;

		AssemblePedals( pedal, Frame, out Entity pedalPivot, out ModelEntity leftPedal, out ModelEntity rightPedal );
		Pedals = pedalPivot;
		LeftPedal = leftPedal;
		RightPedal = rightPedal;

		if ( trail != null )
		{
			trailParticle = Particles.Create( trail.Model, this );
		}

		Scale = .85f;
	}

	private void AssemblePedals( CustomizationPart pedal, ModelEntity frame, out Entity pivot, out ModelEntity leftPedal, out ModelEntity rightPedal )
	{
		pivot = new Entity();
		pivot.SetParent( frame, "hub", Transform.Zero );

		leftPedal = new ModelEntity( pedal.Model );
		leftPedal.SetParent( pivot, null, Transform.Zero );

		rightPedal = new ModelEntity( pedal.Model );
		rightPedal.SetParent( pivot, null, Transform.Zero );

		var pedalHub = leftPedal.GetAttachment( "hub", false ) ?? Transform.Zero;

		leftPedal.Position = (frame.GetAttachment( "pedal_L" ) ?? Transform.Zero).Position;
		rightPedal.Position = (frame.GetAttachment( "pedal_R" ) ?? Transform.Zero).Position;

		leftPedal.LocalPosition -= pedalHub.Position;
		rightPedal.LocalPosition += pedalHub.Position;
		rightPedal.LocalRotation *= Rotation.From( 180, 180, 0 );

		pivot.LocalRotation = pivot.LocalRotation.RotateAroundAxis( Vector3.Right, -90 );
	}

	private int parthash = -1;

	[Event.Tick.Server]
	private void CheckEnsemble()
	{
		if ( Parent is not UnicyclePlayer pl ) return;
		if ( !pl.IsValid() || !pl.Client.IsValid() ) return;

		var cfg = pl.Client.Components.Get<CustomizationComponent>();
		var hash = cfg.GetPartsHash();

		if ( hash == parthash ) return;

		parthash = hash;
		AssembleParts();

		pl.Citizen.Position = GetAssPosition();
	}

	[Event.Tick]
	private void SpinParts()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var pedalAlpha = pl.PedalPosition.LerpInverse( -1f, 1f );
		var targetPitch = 0f.LerpTo( 180, pedalAlpha );
		var targetRot = Rotation.From( targetPitch, 0, 0 );

		if ( IsServer && Pedals.IsValid() )
		{
			var ang = targetRot.Angle() - Pedals.LocalRotation.Angle();
			Pedals.LocalRotation = Pedals.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) * Time.Delta * 10 );
		}

		if ( IsClient && localPawnPedals.IsValid() )
		{
			var ang = targetRot.Angle() - localPawnPedals.LocalRotation.Angle();
			localPawnPedals.LocalRotation = localPawnPedals.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) * Time.Delta * 10 );
		}

		if ( IsServer && WheelPivot.IsValid() )
		{
			var wheelRadius = Wheel.GetAttachment( "hud", false )?.Position.z ?? 12f;
			var angularSpeed = 180f * pl.Velocity.WithZ( 0 ).Length / ((float)Math.PI * wheelRadius);
			var dir = Math.Sign( Vector3.Dot( pl.Velocity.Normal, pl.Rotation.Forward ) );

			WheelPivot.LocalRotation = WheelPivot.LocalRotation.RotateAroundAxis( Vector3.Left, angularSpeed * dir * Time.Delta );
		}
	}

	[Event.Tick.Server]
	private void SetTrailControlPoint()
	{
		if ( Parent is not UnicyclePlayer pl ) return;
		if ( trailParticle == null ) return;

		var a = Math.Min( pl.Velocity.Length / 500f, 1f );
		trailParticle.SetPosition( 6, new Vector3( a, 0, 0 ) );
		trailParticle.SetPosition(8, 1);
	}

	[Event.Tick.Client]
	private void AssembleLocalPedals()
	{
		if ( Parent is not UnicyclePlayer pl || !pl.IsLocalPawn ) return;

		if ( LeftPedal.IsValid() ) LeftPedal.RenderColor = Color.Transparent;
		if ( RightPedal.IsValid() ) RightPedal.RenderColor = Color.Transparent;

		// todo: reassemble if local player equipped a different pedal part
		if ( localPawnPedals.IsValid() ) return;

		var cfg = pl.Client.Components.Get<CustomizationComponent>();
		var pedal = cfg.GetEquippedPart( PartType.Pedal );

		AssemblePedals( pedal, Frame, out localPawnPedals, out localLeftPedal, out localRightPedal );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		localPawnPedals?.Delete();
	}

}

