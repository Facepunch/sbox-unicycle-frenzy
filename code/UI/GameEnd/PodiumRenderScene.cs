
using Sandbox.UI.Construct;
using Sandbox.Diagnostics;

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
		ScenePanel = Add.ScenePanel( SceneWorld, Vector3.Backward * 75 + Vector3.Up * 62, Rotation.Identity, 45 );
		ScenePanel.Camera.Rotation = Rotation.FromPitch( 5 );

		ScenePanel.Style.Width = Length.Percent( 100 );
		ScenePanel.Style.Height = Length.Percent( 100 );

		var citizen = new SceneModel( SceneWorld, "models/citizen/citizen.vmdl", Transform.Zero.WithRotation( Rotation.FromYaw( 180 ) ) );
		new SceneLight( SceneWorld, Vector3.Up * 100, 200f, Color.White * 5 );
		new SceneLight( SceneWorld, Vector3.Backward * 100 + Vector3.Up * 50f, 200f, Color.White * 5 ).ShadowsEnabled = false;

		Dress( citizen, player.Avatar );
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

	private void Dress( SceneModel m, string json )
	{
		var container = new ClothingContainer();
		container.Deserialize( json );
		container.DressSceneObject( m );
	}

}
