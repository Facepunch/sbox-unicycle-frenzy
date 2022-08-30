
using Sandbox;
using SandboxEditor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

[Library( "uf_frenzy_letter" )]
[Display( Name = "FRENZY Collectible", GroupName = "Unicycle Frenzy", Description = "A letter for the F-R-E-N-Z-Y minigame" )]
[HammerEntity]
internal partial class FrenzyCollectible : Collectible
{

	public enum FrenzyCollectibleLetter
	{
		F,
		R,
		E,
		N,
		Z,
		Y
	}

	[Net, Property]
	public FrenzyCollectibleLetter Letter { get; set; }
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

		new FrenzyLetter( this );
		Light = new SpotLightEntity();
		Light.DynamicShadows = false;
		Light.SetParent( this, null, Transform.Zero );
		Light.Position += Vector3.Up * 64;
		Light.Rotation = Rotation.LookAt( Vector3.Down );
		Light.Color = Color.Cyan;
		Light.Brightness = 1f;
	}

	[ClientRpc]
	public static void SetFrenzyLetterCollected( FrenzyCollectibleLetter letter )
	{
		var result = new FrenzyCollectionHelper().TryAdd( letter );
		if ( result == FrenzyCollectionHelper.Result.Added )
		{
			// Log.Error( "ADDED: " + letter );
		}

		if( result == FrenzyCollectionHelper.Result.Completed )
		{
			Achievement.Set( Local.PlayerId, "uf_frenzy", Global.MapName );
		}
	}

	public class FrenzyCollectionHelper
	{

		public enum Result
		{
			None,
			Added,
			Completed
		}

		public Result TryAdd( FrenzyCollectibleLetter letter )
		{
			var cookiename = $"{Global.MapName}.frenzycollection1";
			var collection = Cookie.Get<List<FrenzyCollectibleLetter>>( cookiename, new() );

			if ( collection.Contains( letter ) ) 
				return Result.None;

			collection.Add( letter );

			Cookie.Set( cookiename, collection );

			if ( collection.Count == 6 )
				return Result.Completed;

			return Result.Added;
		}

		public bool Contains( FrenzyCollectibleLetter letter )
		{
			var cookiename = $"{Global.MapName}.frenzycollection1";
			var collection = Cookie.Get<List<FrenzyCollectibleLetter>>( cookiename, new() );

			return collection.Contains( letter );
		}

	}

}
