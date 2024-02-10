
using Sandbox;
using System;

internal class PlayerAnimator : Component
{

	[Property]
	public GameObject LeftPedal { get; set; }
	[Property]
	public GameObject RightPedal { get; set; }
	[Property]
	public GameObject Wheel { get; set; }
	[Property]
	public GameObject PedalPivot { get; set; }
	[Property]
	public GameObject WheelPivot { get; set; }

	UnicycleController Controller;
	SkinnedModelRenderer Model;
	Vector3 InputDirection;

	Vector3 GetInputDirection()
	{
		if ( Controller?.Dead ?? false ) return default;

		var result = Vector3.Zero;
		if ( Input.Down( "lean_left" ) ) result.y = 1;
		if ( Input.Down( "lean_right" ) ) result.y = -1;
		if ( Input.Down( "lean_forward" ) ) result.x = 1;
		if ( Input.Down( "lean_back" ) ) result.x = -1;

		return result;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		InputDirection = GetInputDirection();

		Controller ??= Components.Get<UnicycleController>( FindMode.EverythingInSelfAndChildren );
		Model ??= Components.Get<SkinnedModelRenderer>( FindMode.EverythingInSelfAndChildren );

		if ( Model == null || Controller == null ) return;
		if ( Controller.Dead ) return;

		Model.SceneModel.AnimationGraph = AnimationGraph.Load( "models/citizen_unicycle_frenzy" );

		SpinParts();

		var speed = Controller.Velocity.WithZ( 0 ).Length;
		Model.Set( "move_groundspeed", speed );

		var eyePosition = Model.Transform.Position + Vector3.Up * 64;
		var aimPos = eyePosition + CameraController.ViewAngles.ToRotation().Forward * 200 + Vector3.Up * 50;
		Model.SetLookDirection( "aim_eyes", aimPos - eyePosition );
		Model.SetLookDirection( "aim_head", aimPos - eyePosition );

		if ( LeftPedal != null && RightPedal != null )
		{
			var leftFootAttachment = LeftPedal.Components.Get<Prop>().Model.GetAttachment( "foot" );
			var rightFootAttachment = RightPedal.Components.Get<Prop>().Model.GetAttachment( "foot" );

			// all these transforms are confusing me, should switch to just a gameobject

			var leftFootPos = PedalPivot.Transform.Local.PointToWorld( LeftPedal.Transform.LocalPosition + leftFootAttachment.Value.Position );
			var rightFootPos = PedalPivot.Transform.Local.PointToWorld( RightPedal.Transform.LocalPosition + rightFootAttachment.Value.Position );

			Model.Set( "b_unicycle_enable_foot_ik", true );
			Model.Set( "left_foot_ik.position", Model.Transform.Local.PointToLocal( leftFootPos ) - Vector3.Left * 4 );
			Model.Set( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
			Model.Set( "right_foot_ik.position", Model.Transform.Local.PointToLocal( rightFootPos ) - Vector3.Left * 4 );
			Model.Set( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
		}

		//var a = Controller.PedalPosition.LerpInverse( -1f, 1f ) * .5f;
		//Model.Set( "unicycle_pedaling", a );

		var jumpcharge = Input.Down( "jump" ) ? (Controller.TimeSinceJumpDown / Controller.MaxJumpStrengthTime) : 0f;
		Model.Set( "unicycle_jump_charge", jumpcharge );

		var targetbalx = .5f + (Controller.Tilt.pitch / Controller.MaxLean * .5f + jumpcharge);
		var targetbaly = .5f + (Controller.Tilt.roll / Controller.MaxLean * .5f);
		var balx = Model.GetFloat( "unicycle_balance_x" ).LerpTo( targetbalx, Time.Delta * 3f );
		var baly = Model.GetFloat( "unicycle_balance_y" ).LerpTo( targetbaly, Time.Delta * 3f );
		Model.Set( "unicycle_balance_x", balx );
		Model.Set( "unicycle_balance_y", baly );

		var targetLeanX = InputDirection.x.LerpInverse( -1f, 1f );
		var targetLeanY = 1f - InputDirection.y.LerpInverse( -1f, 1f );
		var leanx = Model.GetFloat( "unicycle_lean_x" ).LerpTo( targetLeanX, Time.Delta * 7f );
		var leany = Model.GetFloat( "unicycle_lean_y" ).LerpTo( targetLeanY, Time.Delta * 7f );
		Model.Set( "unicycle_lean_x", leanx );
		Model.Set( "unicycle_lean_y", leany );
	}

	private void SpinParts()
	{
		if ( Controller == null ) return;
		if ( Wheel == null ) return;
		if ( PedalPivot == null ) return;

		var pedalAlpha = Controller.PedalPosition.LerpInverse( -1f, 1f );
		var targetPitch = 0f.LerpTo( 180, pedalAlpha );
		var targetRot = Rotation.From( targetPitch, 0, 0 );

		var wheelModel = Wheel.Components.Get<ModelRenderer>();
		var wheelHub = wheelModel.Model.GetAttachment( "hub" );
		if ( wheelHub == null ) return;

		var ang = targetRot.Angle() - PedalPivot.Transform.LocalRotation.Angle();
		PedalPivot.Transform.LocalRotation = PedalPivot.Transform.LocalRotation.RotateAroundAxis( Vector3.Left, Math.Abs( ang ) * Time.Delta * 10 );

		var wheelRadius = wheelHub?.Position.z ?? 12f;
		var angularSpeed = 180f * Controller.Velocity.WithZ( 0 ).Length / ((float)Math.PI * wheelRadius);
		var dir = Math.Sign( Vector3.Dot( Controller.Velocity.Normal, Controller.Rotation.Forward ) );

		WheelPivot.Transform.LocalRotation = WheelPivot.Transform.LocalRotation.RotateAroundAxis( Vector3.Left, angularSpeed * dir * Time.Delta );
	}

}
