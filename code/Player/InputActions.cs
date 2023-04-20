
using Sandbox;

public enum InputActions
{
	None,
	Pedal,
	Lean,
	Brake,
	Jump,
	Look,
	BrakeAndLean,
	JumpHigher,
	RestartAtCheckpoint,
	RestartCourse,
	LeftPedal,
	RightPedal,
	Spray,
	Menu,
	Scoreboard,
	LeanLeft,
	LeanRight,
	LeanForward,
	LeanBackward
}

public static class InputExtra
{
	public static bool Pressed( InputActions action )
	{
		return Input.Pressed( InputActionsExtensions.GetInputButton( action ) );
	}
}

public static class InputActionsExtensions
{

	public static bool Pressed( this InputActions action ) => Input.Pressed( GetInputButton( action ) );
	public static bool Released( this InputActions action ) => Input.Released( GetInputButton( action ) );
	public static bool Down( this InputActions action ) => Input.Down( GetInputButton( action ) );
	public static string GetButtonOrigin( this InputActions action ) => Input.GetButtonOrigin( GetInputButton( action ) );
	public static string Button( this InputActions action ) => Input.GetButtonOrigin( GetInputButton( action ) );

	public static string GetInputButton( InputActions action )
	{
		return action switch
		{
			InputActions.Brake => "brake",
			InputActions.Jump => "jump",
			InputActions.RestartAtCheckpoint => "restart_checkpoint",
			InputActions.RestartCourse => "restart_course",
			InputActions.LeftPedal => "pedal_left",
			InputActions.RightPedal => "pedal_right",
			InputActions.Spray => "spray",
			InputActions.Menu => "menu",
			InputActions.Scoreboard => "scoreboard",
			InputActions.LeanLeft => "lean_left",
			InputActions.LeanRight => "lean_right",
			InputActions.LeanForward => "lean_forward",
			InputActions.LeanBackward => "lean_backward",
			_ => default
		};
	}

}
