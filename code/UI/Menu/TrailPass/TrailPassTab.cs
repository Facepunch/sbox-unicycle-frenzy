using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
[NavigatorTarget( "menu/trailpass" )]
internal class TrailPassTab : Panel
{

	public Panel SceneCanvas { get; set; }
	public Panel ItemCanvas { get; set; }
	public Panel ExperienceFill { get; set; }
	public Label ExperienceLabel { get; set; }

	private SceneWorld sceneWorld;
	private ScenePanel renderScene;

	private void Rebuild()
	{
		BuildRenderScene();
	}

	private int setxp;
	public override void Tick()
	{
		base.Tick();

		var progress = TrailPassProgress.Current;

		if ( progress.Experience == setxp ) return;
		setxp = progress.Experience;

		var pass = TrailPass.Current;
		UpdateExperienceBar( progress.Experience, pass.MaxExperience );
	}

	private void UpdateExperienceBar( int current, int max )
	{
		var fillPercent = ( (float)current / max) * 100;
		ExperienceFill.Style.Width = Length.Percent( fillPercent );
		ExperienceLabel.Text = $"{current} xp";
	}

	private void BuildRenderScene()
	{
		renderScene?.Delete();
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		new SceneModel( sceneWorld, "models/scene/scene_unicycle_trialpass_main.vmdl", Transform.Zero.WithScale( .35f ) );

		new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 200.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, Color.White * 20.0f );

		renderScene = Add.ScenePanel( sceneWorld, Vector3.Zero, Rotation.From( Angles.Zero ), 75 );
		renderScene.Style.Width = Length.Percent( 100 );
		renderScene.Style.Height = Length.Percent( 100 );
		renderScene.Camera.Position = new Vector3( -15, 0, 58 );
		renderScene.Camera.Rotation = Rotation.From( 12, 0, 0 );
		renderScene.Parent = SceneCanvas;
		//renderSceneAngles = renderScene.CameraRotation.Angles();

		var uicyce = new SceneModel( sceneWorld, "models/unicycle_dev.vmdl", Transform.Zero.WithScale( .2f ) );
		uicyce.Position = renderScene.Camera.Position + renderScene.Camera.Rotation.Forward * 35;
		uicyce.Position = uicyce.Position + renderScene.Camera.Rotation.Right;
		uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Up, 90 );
		uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Left, 10 );
	}

	public override void OnHotloaded() => Rebuild();
	protected override void PostTemplateApplied() => Rebuild();

}
