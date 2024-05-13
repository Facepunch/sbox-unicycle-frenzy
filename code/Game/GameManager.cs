public static partial class GameManager
{
	/// <summary>
	/// Is the game paused?
	/// </summary>
	public static bool IsPaused
	{
		get
		{
			return Game.ActiveScene.TimeScale <= 0f;
		}
		set
		{
			Game.ActiveScene.TimeScale = value ? 0 : 1;
		}
	}
}
