using Sandbox;
using Sandbox.UI;

[UseTemplate]
[NavigatorTarget("menu/controls")]
internal class ControlsTab : Panel
{

	public Panel TutorialButton { get; set; }

	public string BrakeBind => InputActions.Brake.GetButtonOrigin() ?? "UNSET";
	public string LeftPedalBind => InputActions.LeftPedal.GetButtonOrigin() ?? "UNSET";
	public string RightPedalBind => InputActions.RightPedal.GetButtonOrigin() ?? "UNSET";
	public string JumpBind => InputActions.Jump.GetButtonOrigin() ?? "UNSET";
	public string LeanBind => GetLeanBindString();
	public string RestartBind => InputActions.RestartCourse.GetButtonOrigin() ?? "UNSET";
	public string GoBackBind => InputActions.RestartAtCheckpoint.GetButtonOrigin() ?? "UNSET";
	public string SprayBind => InputActions.Spray.GetButtonOrigin() ?? "UNSET";

	public ControlsTab()
	{
		TutorialButton.Style.Display = Global.IsListenServer ? DisplayMode.Flex : DisplayMode.None;
	}

	private string GetLeanBindString()
	{
		var fwd = Input.GetButtonOrigin( InputButton.Forward ) ?? "UNSET";
		var left = Input.GetButtonOrigin( InputButton.Left ) ?? "UNSET";
		var back = Input.GetButtonOrigin( InputButton.Back ) ?? "UNSET";
		var right = Input.GetButtonOrigin( InputButton.Right ) ?? "UNSET";

		return $"{fwd} {left} {back} {right}".ToUpper();
	}

	public void TryPlayTutorial()
	{
		if ( !Global.IsListenServer ) return;

		UnicycleFrenzy.ServerCmd_ChangeMap( "facepunch.uf_tutorial" );
	}

}

