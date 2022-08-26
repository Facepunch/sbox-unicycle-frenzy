using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class TutorialHud : Panel
{

	public Panel ControlHints { get; set; }
	public ControlHint LeftHint { get; set; }
	public ControlHint MiddleHint { get; set; }
	public ControlHint RightHint { get; set; }

	private InputActions currentAction;

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not UnicyclePlayer pl )
		{
			ControlHints.SetClass( "open", false );
			return;
		}

		if ( pl.DisplayedAction == currentAction )
			return;

		currentAction = pl.DisplayedAction;

		if ( pl.DisplayedAction == InputActions.None )
		{
			ControlHints.SetClass( "open", false );
			return;
		}

		ControlHints.SetClass( "open", true );
		LeftHint.SetClass( "open", false );
		RightHint.SetClass( "open", false );
		MiddleHint.SetClass( "open", false );

		switch ( pl.DisplayedAction )
		{
			case InputActions.Pedal:
				LeftHint.Set( "Left Pedal", InputActions.LeftPedal.Button() );
				RightHint.Set( "Right Pedal", InputActions.RightPedal.Button() );
				break;
			case InputActions.Brake:
				MiddleHint.Set( "Brake", InputActions.Brake.Button() );
				break;
			case InputActions.Jump:
				MiddleHint.Set( "Jump", InputActions.Jump.Button() );
				break;
			case InputActions.Look:
				MiddleHint.Set( "Look/Steer", pl.DisplayedAction );
				break;
			case InputActions.Lean:
				MiddleHint.Set( "Lean", pl.DisplayedAction );
				break;
			case InputActions.BrakeAndLean:
				LeftHint.Set( "Brake", InputActions.Brake.Button() );
				RightHint.Set( "Lean", InputActions.Lean );
				break;
			case InputActions.JumpHigher:
				MiddleHint.Set( "Hold to charge jump", InputActions.Jump.Button(), true );
				break;
			default:
				MiddleHint.Set( "UNKNOWN: " + pl.DisplayedAction.ToString(), InputActions.Jump.Button() );
				break;
		}
	}

}
