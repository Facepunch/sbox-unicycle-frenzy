using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
internal class UnicycleLean : Panel
{
	public Label LeftLean;

	public Label RightLean;

	public static UnicycleLean Current { get; private set; }
	public UnicycleLean()
	{
		Current = this;

		LeftLean = Add.Label( "", "leftlean" );
		RightLean = Add.Label( "", "rightlean" );
	}

	public override void Tick()
	{
		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var maxLean = controller.MaxLean;

		var absLean = player.Rotation.Angles();
		var localLean = player.Tilt;
		var localLeftRollAlpha = localLean.roll.LerpInverse( -maxLean, maxLean );
		var localRightRollAlpha = localLean.roll.LerpInverse( maxLean, -maxLean );


		LeftLean.Style.Opacity = MathX.Lerp( .5f, -1, localLeftRollAlpha );
		RightLean.Style.Opacity = MathX.Lerp( .5f, -1, localRightRollAlpha );
	}
}
