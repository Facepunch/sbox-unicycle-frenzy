﻿using Sandbox;
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
		BuildItemList();
	}

	private int setxp;
	public override void Tick()
	{
		base.Tick();

		var progress = TrailPassProgress.CurrentSeason;

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

	private void BuildItemList()
	{
		ItemCanvas.DeleteChildren();

		var trailpass = TrailPass.Current;

		foreach( var item in trailpass.Items )
		{
			var itemicon = new TrailPassItemIcon( item );
			itemicon.Parent = ItemCanvas;
		}
	}

	private void BuildRenderScene()
	{
		renderScene?.Delete();
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		new SceneModel( sceneWorld, "models/room.vmdl", Transform.Zero.WithScale( 2 ).WithPosition( Vector3.Down * 4 ) );

		new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, Color.White * 20.0f );

		renderScene = Add.ScenePanel( sceneWorld, Vector3.Zero, Rotation.From( Angles.Zero ), 75 );
		renderScene.Style.Width = Length.Percent( 100 );
		renderScene.Style.Height = Length.Percent( 100 );
		renderScene.CameraPosition = new Vector3( -53, 100, 42 );
		renderScene.CameraRotation = Rotation.From( 12, 0, 0 );
		renderScene.Parent = SceneCanvas;
		//renderSceneAngles = renderScene.CameraRotation.Angles();

		var uicyce = new SceneModel( sceneWorld, "models/unicycle_dev.vmdl", Transform.Zero.WithScale( .1f ) );
		uicyce.Position = renderScene.CameraPosition + renderScene.CameraRotation.Forward * 15;
		uicyce.Position = uicyce.Position + renderScene.CameraRotation.Left;
		uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Up, 90 );
		uicyce.Rotation = uicyce.Rotation.RotateAroundAxis( Vector3.Right, 10 );
	}

	public override void OnHotloaded() => Rebuild();
	protected override void PostTemplateApplied() => Rebuild();

}
