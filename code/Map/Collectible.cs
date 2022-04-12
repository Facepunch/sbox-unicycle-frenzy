using Hammer;
using Sandbox;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

[Library( "uf_collectible" )]
[Display( Name = "Unicycle Frenzy Collectible", GroupName = "Unicycle Frenzy", Description = "A prop that can be collected." )]
internal partial class Collectible : UfProp
{

	[Net, Property( "Collection", "Discretionary name of the collection group this collectible belongs to." )]
	public string Collection { get; set; }
	[Net]
	public bool Touched { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		EnableAllCollisions = true;
		EnableSolidCollisions = false;

		CollisionGroup = CollisionGroup.Trigger;
	}

	public void SetTouched( bool touched )
	{
		Host.AssertServer();

		Touched = touched;

		if( touched )
		{
			// TODO! BUG! Setting final scale to 0 causes
			// the player to longer collide with the world and fall through map
			// seems... weird?  this ent has nothing to do with player like that
			Juice.Scale( 1, 1.5f, 0.01f )
				.WithTarget( this )
				.WithDuration( .5f );

			Sound.FromEntity( "collect", this );
		}
		else
		{
			Juice.Scale( 0, 1.5f, 1f )
				.WithTarget( this )
				.WithDuration( .5f );
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

		Event.Run( "collection.collected", this );

		if ( IsCollected( Collection ) )
		{
			Event.Run( "collection.complete", Collection );
		}
	}

	public static bool IsCollected( string collection )
	{
		var ents = All
			.OfType<Collectible>()
			.Where( x => x.IsValid() && x.Collection.Equals( collection, StringComparison.InvariantCultureIgnoreCase ) );

		if ( !ents.Any() ) return false;

		return ents.All( x => x.Touched );
	}

	public static void ResetCollection( string collection )
	{
		var ents = All
			.OfType<Collectible>()
			.Where( x => x.IsValid() && x.Collection.Equals( collection, StringComparison.InvariantCultureIgnoreCase ) );

		foreach( var ent in ents )
		{
			ent.SetTouched( false );
		}

		Event.Run( "collection.reset", collection );
	}

}
