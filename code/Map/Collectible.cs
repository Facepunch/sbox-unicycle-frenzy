
using Sandbox;
using Editor;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

[Library( "uf_collectible" )]
[Display( Name = "Unicycle Frenzy Collectible", GroupName = "Unicycle Frenzy", Description = "A prop that can be collected." )]
[HammerEntity]
internal partial class Collectible : UfProp
{

	[Net, Property( "Collection", "Discretionary name of the collection group this collectible belongs to." )]
	public string Collection { get; set; }
	[Net]
	public bool Touched { get; set; }

	private float OriginalZ;

	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Static );
		EnableAllCollisions = false;
		EnableTouch = true;

		Tags.Add( "trigger" );
		
		Transmit = TransmitType.Always;
		OriginalZ = Position.z;
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		var offset = 7f * MathF.Sin( Time.Now * 1.35f );
		Position = Position.WithZ( OriginalZ + offset );

		Rotation *= Rotation.FromYaw( 2f );
	}

	public void SetTouched( bool touched )
	{
		Game.AssertServer();

		Touched = touched;

		if( touched )
		{
			Particles.Create( "particles/misc/collectpickup.vpcf", Position );
			Sound.FromEntity( "collect", this );
		}
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( Touched ) return;
		if ( IsClient ) return;
		if ( other is not UnicyclePlayer pl ) return;
		if ( pl.Fallen ) return;

		SetTouched( true );

		OnCollected( pl );
	}

	public static bool IsCollected( string collection )
	{
		var ents = All
			.OfType<Collectible>()
			.Where( x => x.IsValid() && !string.IsNullOrEmpty( x.Collection ) )
			.Where( x => x.Collection.Equals( collection, StringComparison.InvariantCultureIgnoreCase ) );

		if ( !ents.Any() ) return false;

		return ents.All( x => x.Touched );
	}

	public static void ResetCollection( string collection )
	{
		var ents = All
			.OfType<Collectible>()
			.Where( x => x.IsValid() && !string.IsNullOrEmpty( x.Collection ) )
			.Where( x => x.Collection.Equals( collection, StringComparison.InvariantCultureIgnoreCase ) );

		foreach( var ent in ents )
		{
			ent.SetTouched( false );
			ent.SetRenderAlphaRecursive( 1f );
		}

		Event.Run( "collection.reset", collection );
	}

	protected virtual void OnCollected( UnicyclePlayer player )
	{
		Event.Run( "collection.collected", this );

		if ( IsCollected( Collection ) )
		{
			OnCollectionComplete();

			Event.Run( "collection.complete", Collection );
		}

		this.SetRenderAlphaRecursive( 0f );
	}

	protected virtual void OnCollectionComplete() { }

}
