﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

[UseTemplate]
internal class UnicycleLean : Panel
{

	public Panel LeftLean { get; set; }
	public Panel RightLean { get; set; }
	public Panel AbsLean { get; set; }
	public Panel LocalLean { get; set; }
	
	public override void Tick()
	{
		if ( Local.Pawn is not UnicyclePlayer player ) return;
		if ( player.Controller is not UnicycleController controller ) return;

		var maxLean = controller.MaxLean;

		var absLean = player.Rotation.Angles();
		var absRollAlpha = absLean.roll.LerpInverse( -maxLean, maxLean );
		var absPitchAlpha = absLean.pitch.LerpInverse( maxLean, -maxLean );

		AbsLean.Style.Left = Length.Percent( absRollAlpha * 100f );
		AbsLean.Style.Top = Length.Percent( absPitchAlpha * 100f );

		var leftAlpha = absRollAlpha.LerpInverse( .5f, 0f );
		var rightAlpha = absRollAlpha.LerpInverse( .5f, 1f );
		LeftLean.Style.Opacity = leftAlpha;
		RightLean.Style.Opacity = rightAlpha;

		Style.Opacity = Math.Max( leftAlpha, rightAlpha );

		var localLean = player.Tilt;
		var localRollAlpha = localLean.roll.LerpInverse( -maxLean, maxLean );
		var localPitchAlpha = localLean.pitch.LerpInverse( maxLean, -maxLean );

		LocalLean.Style.Left = Length.Percent( localRollAlpha * 100f );
		LocalLean.Style.Top = Length.Percent( localPitchAlpha * 100f );
	}
}
