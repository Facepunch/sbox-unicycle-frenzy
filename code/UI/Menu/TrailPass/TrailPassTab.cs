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
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 5 );

		renderScene = Add.ScenePanel( sceneWorld, Vector3.Zero, Rotation.From( Angles.Zero ), 75 );
		renderScene.Style.Width = Length.Percent( 100 );
		renderScene.Style.Height = Length.Percent( 100 );
		renderScene.Camera.Position = new Vector3( -15, 0, 58 );
		renderScene.Camera.Rotation = Rotation.From( 12, 0, 0 );
		renderScene.Camera.BackgroundColor = Color.White;
		renderScene.Parent = SceneCanvas;

		renderScene.World.GradientFog.Enabled = true;
		renderScene.World.GradientFog.Color = new Color32( 57, 48, 69 );
		renderScene.World.GradientFog.MaximumOpacity = 0.8f;
		renderScene.World.GradientFog.StartHeight = 10;
		renderScene.World.GradientFog.EndHeight = 9000;
		renderScene.World.GradientFog.DistanceFalloffExponent = 3;
		renderScene.World.GradientFog.VerticalFalloffExponent = 3;
		renderScene.World.GradientFog.StartDistance = 50;
		renderScene.World.GradientFog.EndDistance = 200;
	}

	public override void OnHotloaded() => Rebuild();
	protected override void PostTemplateApplied() => Rebuild();

}
