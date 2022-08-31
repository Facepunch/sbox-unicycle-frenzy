
using Sandbox;
using SandboxEditor;
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
		Host.AssertServer();

		Touched = touched;

		if( touched )
		{
			Particles.Create( "particles/misc/collectpickup.vpcf", Position );

			Juice.Scale( 1, 1.15f, 0.01f )
				.WithTarget( this )
				.WithDuration( .15f );

			Sound.FromEntity( "collect", this );
		}
		else
		{
			Juice.Scale( 0, 1.5f, 1f )
				.WithTarget( this )
				.WithDuration( .15f );
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
	}

	protected virtual void OnCollectionComplete() { }

}
