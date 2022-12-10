
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

internal class StopTriggerHint : Panel
{
	public Label Timer { get; protected set; }
	//public RadialFill Fill { get; protected set; }

	public StopTriggerHint()
	{
		Timer = Add.Label();
		//Fill = AddChild<RadialFill>();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Game.LocalPawn is not UnicyclePlayer pl )
			return;

		var wasopen = HasClass( "open" );
		var shouldShow = pl.TouchingStopDoorTrigger && UnicyclePlayer.StopDoor?.State == DoorEntity.DoorState.Closed;
		SetClass( "open", shouldShow );

		if ( !shouldShow ) return;

		if ( !wasopen )
		{
			Sound.FromScreen( "sounds/misc/tutorial.hint.sound" );
		}

		var shouldFill = pl.StopDoorTimer > 0 && pl.StopDoorTimer < 1f;

		//Fill.FillColor = Color.White;
		//Fill.TrackColor = Color.White.WithAlpha( .25f );
		//Fill.FillStart = 0f;
		//Fill.FillAmount = shouldFill ? pl.StopDoorTimer : .01f;
	}

}
