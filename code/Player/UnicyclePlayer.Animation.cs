using Sandbox;

internal partial class UnicyclePlayer
{
	void UpdateAnimation()
	{
		if ( !Citizen.IsValid() ) return;
		if ( !Unicycle.IsValid() ) return;

		var citizen = Citizen;
		var unicycle = Unicycle;
		
		var speed = Velocity.WithZ( 0 ).Length;
		citizen.SetAnimParameter( "move_groundspeed", speed );

		var aimPos = EyePosition + ViewAngles.ToRotation().Forward * 200 + Vector3.Up * 50;
		citizen.SetAnimLookAt( "aim_eyes", EyePosition, aimPos );
		citizen.SetAnimLookAt( "aim_head", EyePosition, aimPos );

		var leftpos = unicycle.DisplayedLeftPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
		var rightpos = unicycle.DisplayedRightPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
		leftpos = citizen.Transform.PointToLocal( leftpos + Rotation.Up * 5 );
		rightpos = citizen.Transform.PointToLocal( rightpos + Rotation.Up * 5 );

		citizen.SetAnimParameter( "b_unicycle_enable_foot_ik", true );
		citizen.SetAnimParameter( "left_foot_ik.position", leftpos );
		citizen.SetAnimParameter( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
		citizen.SetAnimParameter( "right_foot_ik.position", rightpos );
		citizen.SetAnimParameter( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );

		var a = PedalPosition.LerpInverse( -1f, 1f ) * .5f;
		citizen.SetAnimParameter( "unicycle_pedaling", a );

		if ( Controller is not UnicycleController ctrl ) return;

		var jumpcharge = InputActions.Jump.Down() ? (TimeSinceJumpDown / ctrl.MaxJumpStrengthTime) : 0f;
		citizen.SetAnimParameter( "unicycle_jump_charge", jumpcharge );

		var targetbalx = .5f + (Tilt.pitch / ctrl.MaxLean * .5f + jumpcharge);
		var targetbaly = .5f + (Tilt.roll / ctrl.MaxLean * .5f);
		var balx = citizen.GetAnimParameterFloat( "unicycle_balance_x" ).LerpTo( targetbalx, Time.Delta * 3f );
		var baly = citizen.GetAnimParameterFloat( "unicycle_balance_y" ).LerpTo( targetbaly, Time.Delta * 3f );
		citizen.SetAnimParameter( "unicycle_balance_x", balx );
		citizen.SetAnimParameter( "unicycle_balance_y", baly );

		var targetLeanX = InputDirection.x.LerpInverse( -1f, 1f );
		var targetLeanY = 1f - InputDirection.y.LerpInverse( -1f, 1f );
		var leanx = citizen.GetAnimParameterFloat( "unicycle_lean_x" ).LerpTo( targetLeanX, Time.Delta * 7f );
		var leany = citizen.GetAnimParameterFloat( "unicycle_lean_y" ).LerpTo( targetLeanY, Time.Delta * 7f );
		citizen.SetAnimParameter( "unicycle_lean_x", leanx );
		citizen.SetAnimParameter( "unicycle_lean_y", leany );
	}

}

