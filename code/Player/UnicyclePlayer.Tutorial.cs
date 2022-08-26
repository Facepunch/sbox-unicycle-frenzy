﻿using Sandbox;
using Sandbox.Component;
using System.Linq;

partial class UnicyclePlayer
{

	[Net]
	public InputActions DisplayedAction { get; set; }
	[Net]
	public bool PerfectPedalGlow { get; set; }
	[Net]
	public float StopDoorTimer { get; set; }
	[Net]
	public bool TouchingStopDoorTrigger { get; set; }

	private TimeSince tsVelocityLow;

	[Event.Frame]
	private void OnFrame()
	{
		if ( !Unicycle.IsValid() ) return;

		// pedal glow
		var glowLeftPedal = PerfectPedalGlow; 
		var glowRightPedal = PerfectPedalGlow;

		if ( Controller is UnicycleController c )
		{
			c.CanPedalBoost( out bool leftPedal, out bool rightPedal );
			glowLeftPedal = glowLeftPedal && leftPedal;
			glowRightPedal = glowRightPedal && rightPedal;
		}

		if( Unicycle.DisplayedLeftPedal.IsValid() && Unicycle.DisplayedRightPedal.IsValid() )
		{
			var leftGlow = Unicycle.DisplayedLeftPedal.Components.GetOrCreate<Glow>();
			var rightGlow = Unicycle.DisplayedRightPedal.Components.GetOrCreate<Glow>();
			leftGlow.Active = glowLeftPedal;
			leftGlow.Color = Color.Green;
			rightGlow.Active = glowRightPedal;
			rightGlow.Color = Color.Green;
		}
	}

	[Event("collection.complete")]
	public void OnCollectionComplete( string collection )
	{
		if ( !Host.IsServer ) return;
		if ( !string.Equals( collection, "collection_tutorial" ) ) return;

		var ent = Entity.All.FirstOrDefault( x => x is DoorEntity && x.Name == "tut_door" ) as DoorEntity;
		if ( !ent.IsValid() ) return;

		ent.Open();
	}

	const float StopDuration = 2f;
	[Event.Tick.Server]
	private void CheckStopDoorTrigger()
	{
		if( Velocity.WithZ(0).Length > 35 )
		{
			tsVelocityLow = 0;
			StopDoorTimer = -float.Epsilon;
		}

		TouchingStopDoorTrigger = false;

		if ( !StopDoorTrigger.IsValid() || !StopDoor.IsValid() ) return;
		if ( !StopDoorTrigger.TouchingEntities.Contains( this ) ) return;

		TouchingStopDoorTrigger = true;
		StopDoorTimer = tsVelocityLow / StopDuration;

		var openit = tsVelocityLow >= StopDuration
			&& StopDoor.State == DoorEntity.DoorState.Closed;

		if ( openit )
		{
			StopDoor.Open();
			Sound.FromEntity( "collect", this );
		}
	}

	private void ResetTutorial()
	{
		Host.AssertServer();

		Collectible.ResetCollection( "collection_tutorial" );

		if ( CollectionDoor.IsValid() )
		{
			CollectionDoor.Close();
		}

		if ( StopDoor.IsValid() )
		{
			StopDoor.Close();
		}
	}

	public static BaseTrigger StopDoorTrigger => All.FirstOrDefault( x => x.Name.Equals( "tut_trigger_top" ) ) as BaseTrigger;
	public static DoorEntity StopDoor => All.FirstOrDefault( x => x.Name == "tut_door_stop" ) as DoorEntity;
	private static DoorEntity CollectionDoor => All.FirstOrDefault( x => x is DoorEntity && x.Name == "tut_door" ) as DoorEntity;

}
