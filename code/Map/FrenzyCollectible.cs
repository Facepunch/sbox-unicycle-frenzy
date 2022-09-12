
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

[Library( "uf_frenzy_letter" )]
[Display( Name = "FRENZY Collectible", GroupName = "Unicycle Frenzy", Description = "A letter for the F-R-E-N-Z-Y minigame" )]
[HammerEntity]
internal partial class FrenzyCollectible : Collectible
{

	public enum FrenzyLetter
	{
		F,
		R,
		E,
		N,
		Z,
		Y
	}

	[Net, Property]
	public FrenzyLetter Letter { get; set; }
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

	public bool IsHidden()
	{
		if ( Local.Pawn is not UnicyclePlayer pl )
			return false;

		if ( Holders.Contains( pl ) )
			return true;

		if ( Owners.Contains( pl ) )
			return true;

		return false;
	}

	[Event.Frame]
	private void OnFrame()
	{
		if ( Local.Pawn is not UnicyclePlayer pl )
			return;

		Light.Enabled = !IsHidden();
	}

	[Event( "unicycle.checkpoint.touch" )]
	public void OnPlayerCheckpoint( UnicyclePlayer player )
	{
		if ( !Holders.Contains( player ) ) 
			return;

		Holders.Remove( player );
		Owners.Add( player );

		SetFrenzyLetterCollected( To.Single( player ), Letter );
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

		new global::FrenzyLetter( this );
		Light = new SpotLightEntity();
		Light.DynamicShadows = false;
		Light.SetParent( this, null, Transform.Zero );
		Light.Position += Vector3.Up * 64;
		Light.Rotation = Rotation.LookAt( Vector3.Down );
		Light.Color = Color.Cyan;
		Light.Brightness = 1f;
	}

	[ClientRpc]
	public static void SetFrenzyLetterCollected( FrenzyLetter letter )
	{
		var result = FrenzyCollectionHelper.TryAdd( letter );
		if ( result == FrenzyCollectionHelper.Result.Added )
		{
			// Log.Error( "ADDED: " + letter );
		}

		if( result == FrenzyCollectionHelper.Result.Completed )
		{
			Achievement.Set( Local.PlayerId, "uf_frenzy", Global.MapName );
		}
	}

}
