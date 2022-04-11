
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
	Scoreboard
}

public static class InputActionsExtensions
{

	public static bool Pressed( this InputActions action ) => Input.Pressed( GetInputButton( action ) );
	public static bool Released( this InputActions action ) => Input.Released( GetInputButton( action ) );
	public static bool Down( this InputActions action ) => Input.Down( GetInputButton( action ) );
	public static string GetButtonOrigin( this InputActions action ) => Input.GetButtonOrigin( GetInputButton( action ) );
	public static InputButton Button( this InputActions action ) => GetInputButton( action );

	public static bool Pressed( this InputBuilder b, InputActions action )
	{
		return b.Pressed( GetInputButton( action ) );
	}

	public static InputButton GetInputButton( InputActions action )
	{
		if ( Input.UsingController )
		{
			return action switch
			{
				InputActions.Brake => InputButton.SlotPrev,
				InputActions.Jump => InputButton.SlotNext,
				InputActions.RestartAtCheckpoint => InputButton.Reload,
				InputActions.RestartCourse => InputButton.Use,
				InputActions.LeftPedal => InputButton.Attack2,
				InputActions.RightPedal => InputButton.Attack1,
				InputActions.Spray => InputButton.Flashlight,
				InputActions.Menu => InputButton.Menu,
				InputActions.Scoreboard => InputButton.Score,
				_ => default
			};
		}

		return action switch
		{
			InputActions.Brake => InputButton.Run,
			InputActions.Jump => InputButton.Jump,
			InputActions.RestartAtCheckpoint => InputButton.Reload,
			InputActions.RestartCourse => InputButton.Drop,
			InputActions.LeftPedal => InputButton.Attack1,
			InputActions.RightPedal => InputButton.Attack2,
			InputActions.Spray => InputButton.Flashlight,
			InputActions.Menu => InputButton.Menu,
			InputActions.Scoreboard => InputButton.Score,
			_ => default
		};
	}

}
