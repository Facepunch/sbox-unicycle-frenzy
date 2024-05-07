using Sandbox;

public sealed class TutorialScreen : Component
{
	[Property] public GameObject WelcomeCamera { get; set; }
	[Property] public GameObject FirstTutorial { get; set; }
	[Property] public TutorialMap TutorialMap { get; set; }
	GameObject camera;
	protected override void OnStart()
	{
		base.OnStart();

		camera = Scene.GetAllComponents<CameraComponent>().Where(c => c.IsMainCamera).FirstOrDefault().GameObject;
		camera.Enabled = false;
		FirstTutorial.Enabled = false;

		var player = Scene.GetAllComponents<UnicycleController>().FirstOrDefault();
		player.FreezePlayer = true;

		var screen = Scene.GetAllComponents<ScreenPanel>().FirstOrDefault();
		if ( screen == null ) return;
		var hint = screen.Components.Create<TutorialWelcome>();
		hint.tutorial = this;
	}
	protected override void OnUpdate()
	{

	}

	public void EndTutorial()
	{
		FirstTutorial.Enabled = true;
		camera.Enabled = true;
		WelcomeCamera.Enabled = true;

		var player = Scene.GetAllComponents<UnicycleController>().FirstOrDefault();
		player.FreezePlayer = false;

		TutorialMap.ShowTutorial = false;

		foreach (var zone in Scene.GetAllComponents<BaseZone>())
		{
			zone.EndTutorial();
		}

		var screen = Scene.GetAllComponents<Hud>().FirstOrDefault();
		screen.ShowHud();
	}

	public void StartTutorial()
	{
		FirstTutorial.Enabled = true;
		camera.Enabled = true;
		WelcomeCamera.Enabled = true;

		var player = Scene.GetAllComponents<UnicycleController>().FirstOrDefault();
		player.FreezePlayer = false;
	}
}
