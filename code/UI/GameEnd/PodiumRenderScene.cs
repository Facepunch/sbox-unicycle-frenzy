
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

		var citizen = new SceneModel( SceneWorld, "models/citizen/citizen.vmdl", Transform.Zero.WithRotation( Rotation.FromYaw( 180 ) ) );
		new SceneLight( SceneWorld, Vector3.Backward * 50 + Vector3.Up * 50, 200f, Color.White * 20 );
		new SceneLight( SceneWorld, Vector3.Left * 50, 200f, Color.White * 20 );

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
		var container = new Clothing.Container();
		container.Deserialize( json );

		m.SetMaterialGroup( "Skin01" );

		foreach ( var c in container.Clothing )
		{
			if ( c.Model == "models/citizen/citizen.vmdl" )
			{
				m.SetMaterialGroup( c.MaterialGroup );
				continue;
			}

			var model = Model.Load( c.Model );

			var anim = new SceneModel( SceneWorld, model, m.Transform );

			if ( !string.IsNullOrEmpty( c.MaterialGroup ) )
				anim.SetMaterialGroup( c.MaterialGroup );

			m.AddChild( "clothing", anim );

			anim.Update( 1.0f );
		}

		foreach ( var group in container.GetBodyGroups() )
		{
			m.SetBodyGroup( group.name, group.value );
		}
	}

}
