using Facepunch.Customization;
using Sandbox;
using System;

internal partial class UnicycleEntity : Entity
{

	[Net]
	public ModelEntity FrameModel { get; set; }
	[Net]
	public ModelEntity SeatModel { get; set; }
	[Net]
	public ModelEntity WheelModel { get; set; }
	[Net]
	public ModelEntity LeftPedalModel { get; set; }
	[Net]
	public ModelEntity RightPedalModel { get; set; }
	[Net]
	public Entity PedalsPivot { get; set; }
	[Net]
	public Entity WheelPivot { get; set; }
	[Net]
	public float WheelRadius { get; set; }

	private Particles trailParticle;
	private Entity localPawnPedals;
	private ModelEntity localLeftPedal;
	private ModelEntity localRightPedal;

	public ModelEntity DisplayedLeftPedal => localLeftPedal ?? LeftPedalModel;
	public ModelEntity DisplayedRightPedal => localRightPedal ?? RightPedalModel;

	public Vector3 GetAssPosition()
	{
		var assAttachment = SeatModel.GetAttachment( "Ass" );
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

		FrameModel?.Delete();
		FrameModel = null;
		trailParticle?.Destroy();
		trailParticle = null;

		var cfg = pl.Client.Components.Get<CustomizationComponent>();

		var frame = cfg.GetEquippedPart( PartType.Frame.ToString() );
		var seat = cfg.GetEquippedPart( PartType.Seat.ToString() );
		var wheel = cfg.GetEquippedPart( PartType.Wheel.ToString() );
		var pedal = cfg.GetEquippedPart( PartType.Pedal.ToString() );
		var trail = cfg.GetEquippedPart( PartType.Trail.ToString() );

		FrameModel = new ModelEntity( frame.AssetPath );
		FrameModel.SetParent( this, null, Transform.Zero );

		SeatModel = new ModelEntity( seat.AssetPath );
		SeatModel.SetParent( FrameModel, "seat", Transform.Zero );

		WheelPivot = new Entity();
		WheelPivot.SetParent( FrameModel, "hub", Transform.Zero );

		WheelModel = new ModelEntity( wheel.AssetPath );
		WheelModel.SetParent( WheelPivot, null, Transform.Zero );

		var wheelHub = WheelModel.GetAttachment( "hub" ) ?? Transform.Zero;
		WheelRadius = wheelHub.Position.z - WheelModel.Position.z;

		WheelModel.LocalPosition -= Vector3.Up * WheelRadius;
		FrameModel.LocalPosition = Vector3.Up * WheelRadius;

		AssemblePedals( pedal, FrameModel, out Entity pedalPivot, out ModelEntity leftPedal, out ModelEntity rightPedal );
		PedalsPivot = pedalPivot;
		LeftPedalModel = leftPedal;
		RightPedalModel = rightPedal;

		if ( trail != null )
		{
			trailParticle = Particles.Create( trail.AssetPath, this );
		}

		//Scale = .85f;
	}

	private void AssemblePedals( CustomizationPart pedal, ModelEntity frame, out Entity pivot, out ModelEntity leftPedal, out ModelEntity rightPedal )
	{
		pivot = new Entity();
		pivot.SetParent( frame, "hub", Transform.Zero );

		leftPedal = new ModelEntity( pedal.AssetPath );
		leftPedal.SetParent( pivot, null, Transform.Zero );

		rightPedal = new ModelEntity( pedal.AssetPath );
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
		//pl.Terry.Position -= Vector3.Up * 4; // remove this when proper ass attachment
	}

	[Event.Tick]
	private void SpinParts()
	{
		if ( Parent is not UnicyclePlayer pl ) return;

		var pedalAlpha = pl.PedalPosition.LerpInverse( -1f, 1f );
		var targetPitch = 0f.LerpTo( 180, pedalAlpha );
		var targetRot = Rotation.From( targetPitch, 0, 0 );

		if ( IsServer && PedalsPivot.IsValid() )
		{
			var ang = targetRot.Angle() - PedalsPivot.LocalRotation.Angle();
			PedalsPivot.LocalRotation = PedalsPivot.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) * Time.Delta * 10 );
		}

		if ( IsClient && localPawnPedals.IsValid() )
		{
			var ang = targetRot.Angle() - localPawnPedals.LocalRotation.Angle();
			localPawnPedals.LocalRotation = localPawnPedals.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) * Time.Delta * 10 );
		}

		if ( IsServer && WheelPivot.IsValid() )
		{
			var angularSpeed = 180f * pl.Velocity.WithZ( 0 ).Length / ((float)Math.PI * WheelRadius);
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

	[Event.Frame]
	private void AssembleLocalPedals()
	{
		if ( Parent is not UnicyclePlayer pl || !pl.IsLocalPawn ) return;

		if ( LeftPedalModel.IsValid() ) LeftPedalModel.RenderColor = Color.Transparent;
		if ( RightPedalModel.IsValid() ) RightPedalModel.RenderColor = Color.Transparent;

		// todo: reassemble if local player equipped a different pedal part
		if ( localPawnPedals.IsValid() ) return;

		var cfg = pl.Client.Components.Get<CustomizationComponent>();
		var pedal = cfg.GetEquippedPart( PartType.Pedal.ToString() );

		AssemblePedals( pedal, FrameModel, out localPawnPedals, out localLeftPedal, out localRightPedal );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		localPawnPedals?.Delete();
	}

}

