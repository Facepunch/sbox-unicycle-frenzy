﻿

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

internal class PodiumRenderScene : Panel
{

	private UnicyclePlayer player;
	private ScenePanel ScenePanel;
	private SceneWorld SceneWorld;

	public PodiumRenderScene( UnicyclePlayer pl )
	{
		player = pl;

		Build();
	}

	private void Build()
	{
		Assert.True( player.IsValid() );

		SceneWorld?.Delete();
		ScenePanel?.Delete();

		SceneWorld = new SceneWorld();
		ScenePanel = Add.ScenePanel( SceneWorld, Vector3.Backward * 75 + Vector3.Up * 52, Rotation.Identity, 75 );
		ScenePanel.CameraRotation = Rotation.FromPitch( 10 );

		ScenePanel.Style.Width = Length.Percent( 100 );
		ScenePanel.Style.Height = Length.Percent( 100 );

		new SceneModel( SceneWorld, "models/citizen/citizen.vmdl", Transform.Zero.WithRotation( Rotation.FromYaw( 180 ) ) );
		new SceneLight( SceneWorld, Vector3.Backward * 50 + Vector3.Up * 50, 200f, Color.White * 10 );
	}

	public override void Tick()
	{
		base.Tick();

		if ( !SceneWorld.IsValid() ) return;

		foreach(var obj in SceneWorld.SceneObjects )
		{
			if ( obj is not SceneModel m ) continue;
			m.Update( RealTime.Delta );
		}
	}

}
