
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;

internal class PartScenePanel : Panel
{

	//
	// todo: replace this with pngs, this is just a quick patch to hide missing part thumbs
	//

	public float RotationSpeed { get; set; }
	public bool LookRight { get; set; }
	public CustomizationPart Part { get; set; }

	private SceneWorld sceneWorld;
	private SceneObject sceneObj;
	private ScenePanel scenePanel;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		Build();
	}

	private void Build()
	{
		if ( Part == null ) return;

		LookRight = Part.PartType == PartType.Wheel || Part.PartType == PartType.Seat;

		scenePanel?.Delete();
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		Style.SetBackgroundImage( string.Empty );
		Style.Width = Length.Percent( 100 );
		Style.Height = Length.Percent( 100 );

		scenePanel = Add.ScenePanel( sceneWorld, Vector3.Zero, Rotation.Identity, 35 );

		if ( !string.IsNullOrEmpty( Part.Particle ) )
		{
			var p = new SceneParticles( sceneWorld, Part.Particle );
			p.SetControlPoint( 6, .75f );
			p.SetControlPoint( 7, 1 );
			p.SetControlPoint( 8, 0 );
			p.Simulate( 100f );

			sceneObj = p;

			scenePanel.Camera.Position = Vector3.Up * 10 + Vector3.Left * 30;
			scenePanel.Camera.Rotation = Rotation.From( 90, 0, 0 );
			scenePanel.Style.Opacity = 1;
			scenePanel.Camera.BackgroundColor = Color.Black;
			scenePanel.Camera.EnablePostProcessing = false;
			scenePanel.Camera.FieldOfView = 25;
		}
		else if ( !string.IsNullOrEmpty( Part.Model ) )
		{
			sceneObj = new SceneModel( sceneWorld, Part.Model, Transform.Zero );
			if ( LookRight ) sceneObj.Rotation = Rotation.LookAt( Vector3.Right );
			var bounds = sceneObj.Model.RenderBounds;

			scenePanel.Camera.Position = GetFocusPosition( bounds, Rotation.Identity, scenePanel.Camera.FieldOfView );
			scenePanel.Camera.Rotation = Rotation.From( 0, 0, 0 );
		}
		else if ( !string.IsNullOrEmpty( Part.Texture ) )
		{
			Style.SetBackgroundImage( Part.Texture );
		}

		new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 15 );
		new SceneLight( sceneWorld, Vector3.Backward * 150.0f, 200.0f, Color.White * 15 );

		scenePanel.Style.Width = Length.Percent( 100 );
		scenePanel.Style.Height = Length.Percent( 100 );
	}

	private Vector3 GetFocusPosition( BBox bounds, Rotation cameraRot, float fov )
	{
		var focusDist = 0.75f;
		var maxSize = new[] { bounds.Size.x, bounds.Size.y, bounds.Size.z }.Max();
		var cameraView = 2.0f * (float)Math.Tan( 0.5f * 0.017453292f * fov );
		var distance = focusDist * maxSize / cameraView;
		distance += 0.5f * maxSize;
		return bounds.Center - distance * cameraRot.Forward;
	}

	[Event.Client.Frame]
	private void OnFrame()
	{
		if ( !sceneObj.IsValid() ) return;

		if( RotationSpeed > 0 )
		{
			sceneObj.Rotation = sceneObj.Rotation.RotateAroundAxis( Vector3.Up, RotationSpeed * Time.Delta );
		}

		if( sceneObj is SceneParticles p )
		{
			p.Simulate( RealTime.Delta );
		}
	}

}
