
using Sandbox;

internal class UnicycleAnimator : PawnAnimator
{

	public override void Simulate()
    {
        if ( Pawn is not UnicyclePlayer pl ) return;
        if ( !pl.Citizen.IsValid() ) return;
        if ( !pl.Unicycle.IsValid() ) return;

        var citizen = pl.Citizen;
        var unicycle = pl.Unicycle;

		var speed = Pawn.Velocity.WithZ( 0 ).Length;
		citizen.SetAnimParameter( "move_groundspeed", speed );

		var aimPos = Pawn.EyePosition + Input.Rotation.Forward * 200 + Vector3.Up * 50;
		citizen.SetAnimLookAt( "aim_eyes", aimPos );
		citizen.SetAnimLookAt( "aim_head", aimPos );

		var leftpos = unicycle.DisplayedLeftPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
        var rightpos = unicycle.DisplayedRightPedal?.GetAttachment( "foot" )?.Position ?? Vector3.Zero;
        leftpos = citizen.Transform.PointToLocal( leftpos + Rotation.Up * 5 );
        rightpos = citizen.Transform.PointToLocal( rightpos + Rotation.Up * 5 );

        citizen.SetAnimParameter( "b_unicycle_enable_foot_ik", true );
        citizen.SetAnimParameter( "left_foot_ik.position", leftpos );
		citizen.SetAnimParameter( "left_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );
		citizen.SetAnimParameter( "right_foot_ik.position", rightpos );
        citizen.SetAnimParameter( "right_foot_ik.rotation", Rotation.From( 90, -90, 0 ) );

		var a = pl.PedalPosition.LerpInverse( -1f, 1f ) * .5f;
		citizen.SetAnimParameter( "unicycle_pedaling", a );

        if ( pl.Controller is not UnicycleController ctrl ) return;

        var jumpcharge = InputActions.Jump.Down() ? (pl.TimeSinceJumpDown / ctrl.MaxJumpStrengthTime) : 0f;
        citizen.SetAnimParameter( "unicycle_jump_charge", jumpcharge );

		var targetbalx = .5f + ( pl.Tilt.pitch / ctrl.MaxLean * .5f );
		var targetbaly = .5f + ( pl.Tilt.roll / ctrl.MaxLean * .5f );
		var balx = citizen.GetAnimParameterFloat( "unicycle_balance_x" ).LerpTo( targetbalx, Time.Delta * 3f );
		var baly = citizen.GetAnimParameterFloat( "unicycle_balance_y" ).LerpTo( targetbaly, Time.Delta * 3f );
		citizen.SetAnimParameter( "unicycle_balance_x", balx );
		citizen.SetAnimParameter( "unicycle_balance_y", baly );

		var targetLeanX = Input.Forward.LerpInverse( -1f, 1f );
		var targetLeanY = 1f - Input.Left.LerpInverse( -1f, 1f );
		var leanx = citizen.GetAnimParameterFloat( "unicycle_lean_x" ).LerpTo( targetLeanX, Time.Delta * 7f );
		var leany = citizen.GetAnimParameterFloat( "unicycle_lean_y" ).LerpTo( targetLeanY, Time.Delta * 7f );
		citizen.SetAnimParameter( "unicycle_lean_x", leanx );
		citizen.SetAnimParameter( "unicycle_lean_y", leany );
	}

}
