using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
internal class UnicycleLean : Panel
{
	public Label LeftLean;

	public Label RightLean;
	
	public Label CenterLean;
	
	public Label PointLean;

	public Panel UFLean { get; set; }
	public Panel AbsLean { get; set; }
	public Panel LocalLean { get; set; }
	public Panel SafeZone { get; set; }

	public static UnicycleLean Current { get; private set; }

	public UnicycleLean()
	{
		Current = this;
	}
	
	public override void Tick()
	{
		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var maxLean = controller.MaxLean;

		//var localLean = player.Tilt;
		//var LeftRollAlpha = localLean.roll.LerpInverse( -maxLean, maxLean );
		//var RightRollAlpha = localLean.roll.LerpInverse( maxLean, -maxLean );

		var absLean = player.Rotation.Angles();
		var absRollAlpha = absLean.roll.LerpInverse( -maxLean, maxLean );
		var absPitchAlpha = absLean.pitch.LerpInverse( maxLean, -maxLean );

		AbsLean.Style.Left = Length.Percent( absRollAlpha * 100f );
		AbsLean.Style.Top = Length.Percent( absPitchAlpha * 100f );


		var localLean = player.Tilt;
		var localRollAlpha = localLean.roll.LerpInverse( -maxLean, maxLean );
		var localPitchAlpha = localLean.pitch.LerpInverse( maxLean, -maxLean );

		LocalLean.Style.Left = Length.Percent( localRollAlpha * 100f );
		LocalLean.Style.Top = Length.Percent( localPitchAlpha * 100f );

		//LeftLean.Style.Opacity = MathX.Lerp( .75f, -1, LeftRollAlpha );
		//RightLean.Style.Opacity = MathX.Lerp( .75f, -1, RightRollAlpha );
	}
}
