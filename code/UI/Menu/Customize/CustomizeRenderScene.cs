﻿
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Linq;

internal class CustomizeRenderScene : Panel
{

	private string prevtrail;
	private SceneObject unicycleObject;
	private SceneParticles trailParticle;
	private SceneWorld sceneWorld;
	private ScenePanel renderScene;
	private Angles renderSceneAngles = new( 25.0f, 0.0f, 0.0f );
	private float renderSceneDistance = 85;
	private Vector3 renderScenePos => Vector3.Up * 22 + renderSceneAngles.Direction * -renderSceneDistance;

	private bool drag;

	public override void OnButtonEvent( ButtonEvent e )
	{
		if ( e.Button == "mouseleft" )
		{
			drag = e.Pressed;
		}

		base.OnButtonEvent( e );
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( renderScene == null ) return;

		if ( drag )
		{
			renderSceneAngles.pitch += Mouse.Delta.y * .5f;
			renderSceneAngles.yaw -= Mouse.Delta.x * .5f;
			renderSceneAngles.pitch = renderSceneAngles.pitch.Clamp( 0, 75 );
		}

		renderScene.Camera.Position = renderScene.Camera.Position.LerpTo( renderScenePos, 10f * Time.Delta );
		renderScene.Camera.Rotation = Rotation.Lerp( renderScene.Camera.Rotation, Rotation.From( renderSceneAngles ), 15f * Time.Delta );

		trailParticle?.Simulate( RealTime.Delta );
	}

	public override void OnMouseWheel( float value )
	{
		renderSceneDistance += value * 3;
		renderSceneDistance = renderSceneDistance.Clamp( 10, 200 );

		base.OnMouseWheel( value );
	}

	private void BuildSceneWorld()
	{
		sceneWorld?.Delete();
		sceneWorld = new SceneWorld();

		new SceneModel( sceneWorld, "models/scene/scene_unicycle_ensemble_main.vmdl", Transform.Zero.WithScale( 1 ).WithPosition( Vector3.Down * 4 ) );

		var skycolor = Color.Orange;

		var sceneLight = Entity.All.FirstOrDefault( x => x is EnvironmentLightEntity ) as EnvironmentLightEntity;
		if ( sceneLight.IsValid() )
		{
			skycolor = sceneLight.SkyColor;
		}

		new SceneLight( sceneWorld, Vector3.Up * 150.0f, 200.0f, Color.White * 5.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Forward * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Backward * 100.0f, 200, Color.White * 15f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Left * 100.0f, 200, skycolor * 20.0f );
		new SceneLight( sceneWorld, Vector3.Up * 75.0f + Vector3.Right * 100.0f, 200, Color.White * 15.0f );
		new SceneLight( sceneWorld, Vector3.Up * 100.0f + Vector3.Up, 200, Color.Yellow * 15.0f );

		renderScene = Add.ScenePanel( sceneWorld, renderScenePos, Rotation.From( renderSceneAngles ), 75 );
		renderScene.Style.Width = Length.Percent( 100 );
		renderScene.Style.Height = Length.Percent( 100 );
		renderScene.Camera.Position = new Vector3( -33, 100, 42 );
		renderScene.Camera.Rotation = Rotation.From( 10, -62, 0 );
		renderSceneAngles = renderScene.Camera.Rotation.Angles();
	}

	public void Build()
	{
		if ( sceneWorld == null ) BuildSceneWorld();

		unicycleObject?.Delete();

		var ensemble = Local.Client.Components.Get<CustomizationComponent>();
		unicycleObject = BuildUnicycleObject( ensemble );
	}

	private SceneObject BuildUnicycleObject( CustomizationComponent ensemble )
	{
		var frameObj = new SceneModel( sceneWorld, GetPartModel( ensemble, PartType.Frame ), Transform.Zero );
		var wheelObj = new SceneModel( sceneWorld, GetPartModel( ensemble, PartType.Wheel ), Transform.Zero );
		var seatObj = new SceneModel( sceneWorld, GetPartModel( ensemble, PartType.Seat ), Transform.Zero );
		var pedalObjL = new SceneModel( sceneWorld, GetPartModel( ensemble, PartType.Pedal ), Transform.Zero );
		var pedalObjR = new SceneModel( sceneWorld, GetPartModel( ensemble, PartType.Pedal ), Transform.Zero );

		var frameHub = frameObj.Model.GetAttachment( "hub" ) ?? Transform.Zero;
		var wheelHub = wheelObj.Model.GetAttachment( "hub" ) ?? Transform.Zero;
		var wheelRadius = wheelHub.Position.z;

		frameObj.Position = Vector3.Up * (wheelRadius - frameHub.Position.z);

		var seatPosition = frameObj.Model.GetAttachment( "seat" )?.Position ?? Vector3.Zero;
		seatObj.Position = seatPosition + frameObj.Position;

		var pedalHub = pedalObjL.Model.GetAttachment( "hub" ) ?? Transform.Zero;

		pedalObjL.Position = (frameObj.Model.GetAttachment( "pedal_L" ) ?? Transform.Zero).Position;
		pedalObjL.Position += frameObj.Position - pedalHub.Position;

		pedalObjR.Position = (frameObj.Model.GetAttachment( "pedal_R" ) ?? Transform.Zero).Position;
		pedalObjR.Position += frameObj.Position + pedalHub.Position;
		pedalObjR.Rotation *= Rotation.From( 180, 180, 0 );
		

		frameObj.AddChild( "wheel", wheelObj );
		frameObj.AddChild( "seat", seatObj );
		frameObj.AddChild( "pedalL", pedalObjL );
		frameObj.AddChild( "pedalR", pedalObjR );

		var trail = ensemble.GetEquippedPart( PartType.Trail );
		if( trail != null && prevtrail != trail.Particle )
		{
			prevtrail = trail.Particle;
			trailParticle?.Delete();
			trailParticle = new SceneParticles( sceneWorld, trail.Particle );
			trailParticle.SetControlPoint( 6, .75f );
			trailParticle.SetControlPoint( 7, 1 );
			trailParticle.SetControlPoint( 8, 0 );
			trailParticle.SetControlPoint( 0, seatPosition );
		}

		Juice.Scale( 1, 1.15f, 1 )
			.WithDuration( .75f )
			.WithEasing( EasingType.BounceOut )
			.WithTarget( frameObj );

		return frameObj;
	}

	private string GetPartModel( CustomizationComponent ensemble, PartType type )
	{
		var part = ensemble.GetEquippedPart( type );
		return part?.Model ?? "models/sbox_props/watermelon/watermelon.vmdl";
	}

}

