using Sandbox;
using Sandbox.UI;
using System;

[UseTemplate]
internal class DevHud : Panel
{

	[ConVar.ClientData( "uf_devhud" )]
	public static bool ShowDevHud { get; set; } = false;

	public Panel LeftPedal { get; set; }
	public Panel RightPedal { get; set; }
	public Panel Jump { get; set; }
	public Panel LeanContainer { get; set; }
	public Panel AbsLean { get; set; }
	public Panel LocalLean { get; set; }
	public Panel SafeZone { get; set; }

	public override void Tick()
	{
		base.Tick();

		if ( !Local.Client.GetClientData<bool>( "uf_devhud" ) )
		{
			Style.Display = DisplayMode.None;
			return;
		}

		Style.Display = DisplayMode.Flex;

		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var leftTop = player.PedalPosition.LerpInverse( 1f, -1f );
		var rightTop = player.PedalPosition.LerpInverse( -1f, 1f );
		var maxLean = controller.MaxLean;

		LeftPedal.Style.Top = Length.Percent( leftTop * 100f );
		RightPedal.Style.Top = Length.Percent( rightTop * 100f );

		var jumpStrength = Math.Min( player.TimeSinceJumpDown / controller.MaxJumpStrengthTime, 1f );
		if ( !Input.Down( InputButton.Jump ) ) jumpStrength = 0;
		jumpStrength = Easing.EaseOut( jumpStrength );
		Jump.Style.Height = Length.Percent( jumpStrength * 100f );

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

		var safezoneSize = controller.LeanSafeZone / controller.MaxLean;
		var boxw = LeanContainer.Box.Rect.width * safezoneSize;
		SafeZone.Style.Width = Length.Pixels( boxw );
		SafeZone.Style.Height = Length.Pixels( boxw );
	}

}
