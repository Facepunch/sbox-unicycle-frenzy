
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

[Library( "uf_frenzy_letter" )]
[Display( Name = "FRENZY Collectible", GroupName = "Unicycle Frenzy", Description = "A letter for the F-R-E-N-Z-Y minigame" )]
[HammerEntity]
internal partial class FrenzyCollectible : Collectible
{

	[Net, Property]
	public string Letter { get; set; }
	[Net, Property]
	public IList<UnicyclePlayer> Holders { get; set; }
	[Net, Property]
	public IList<UnicyclePlayer> Owners { get; set; }

	private SpotLightEntity Light;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromSphere( PhysicsMotionType.Static, Vector3.Up * 32f, 32f );
		Collection = "FRENZY";
	}

	protected override void OnCollected( UnicyclePlayer pl )
	{
		base.OnCollected( pl );

		Holders.Add( pl );
	}

	[Event( "unicycle.checkpoint.touch" )]
	public void OnPlayerCheckpoint( UnicyclePlayer player )
	{
		if ( !Holders.Contains( player ) ) return;
		Holders.Remove( player );
		Owners.Add( player );
	}

	[Event( "unicycle.fall" )]
	public void OnPlayerFall( UnicyclePlayer player )
	{
		if ( !Holders.Contains( player ) ) return;

		Holders.Remove( player );
		Touched = false;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		new FrenzyLetter( this );
		Light = new SpotLightEntity();
		Light.DynamicShadows = false;
		Light.SetParent( this, null, Transform.Zero );
		Light.Position += Vector3.Up * 64;
		Light.Rotation = Rotation.LookAt( Vector3.Down );
		Light.Color = Color.Cyan;
		Light.Brightness = 1f;
	}

}
