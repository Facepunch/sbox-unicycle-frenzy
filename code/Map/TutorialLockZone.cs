using System.Security.Cryptography.X509Certificates;

internal class TutorialLockZone : BaseZone
{
	public bool IsTutorial { get; set; } = true;
	[Property] public bool LockTurning { get; set; }
	[Property] public bool LockPedaling { get; set; }
	[Property] public bool LockBraking { get; set; }
	[Property] public bool LockJumping { get; set; }
	[Property] public bool LockLean { get; set; }
	[Property] public bool LockTilt { get; set; }
	[Property] public TutorialType TutorialType { get; set; } = TutorialType.Lean;

	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );

		if ( !IsTutorial ) return;

		player.LockBraking = LockBraking;
		player.LockJumping = LockJumping;
		player.LockLean = LockLean;
		player.LockPedaling = LockPedaling;
		player.LockTilt = LockTilt;
		player.LockTurning = LockTurning;

		ShowingTutorial();

	}

	public void ShowingTutorial()
	{
	

		var screen = Scene.GetAllComponents<ScreenPanel>().FirstOrDefault();
		if ( screen == null ) return;
		var hint = screen.Components.Create<TutorialHints>();
		hint.ShowTutorial( TutorialType );
		hint.tutorialHint = ResourceLibrary.GetAll<UFTutorialHint>().FirstOrDefault( x => x.TutorialType == TutorialType );
		//screen.Enabled = false;
	}
}

public enum TutorialType
{
	Lean,
	Tilt,
	Turn,
	Pedal,
	Brake,
	Jump
}

[GameResource( "Unicycle Frenzy Tutorial Hint", "uftut", "UnicycleFrenzyTutorial", Icon = "help_outline", IconBgColor = "#2e1236", IconFgColor = "#f0f0f0" )]
public class UFTutorialHint : GameResource
{
	public TutorialType TutorialType { get; set; }
	public string HintTitle { get; set; }
	[TextArea] public string HintText { get; set; }
	public string HintVideo { get; set; }
}
