
using Sandbox;
using System.Collections.Generic;
using System.Linq;

internal class BaseZone : Component
{

	[Property]
	public SoundEvent EnterSound { get; set; }
	[Property]
	public SoundEvent ExitSound { get; set; }
	[Property]
	public BBox Bounds { get; set; } = BBox.FromPositionAndSize( 0, 16 );

	BoxCollider Box;
	List<UnicycleController> LastTouching = new();

	protected virtual Color ZoneColor => Color.White;

	protected override void OnStart()
	{
		base.OnStart();

		Box = Components.Create<BoxCollider>();
		Box.Scale = Bounds.Size;
		Box.Center = Bounds.Center;
		Box.IsTrigger = true;
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Draw.Color = ZoneColor;
		Gizmo.Draw.LineBBox( Bounds );

		if ( Gizmo.Control.BoundingBox( "bbox", Bounds, out var newBounds ) )
		{
			Bounds = newBounds;
		}
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		var touching = Box.Touching;
		var touchingThisFrame = new List<UnicycleController>();
		foreach ( var touch in touching )
		{
			var player = touch.Components.Get<UnicycleController>();
			if ( player == null ) continue;

			touchingThisFrame.Add( player );

			if ( LastTouching.Contains( player ) )
			{
				OnPlayerStay( player );
			}
			else
			{
				LastTouching.Add( player );
				OnPlayerEnter( player );
			}
		}

		for ( int i = LastTouching.Count - 1; i >= 0; i-- )
		{
			var player = LastTouching[i];
			if ( touchingThisFrame.Contains( player ) ) continue;
			LastTouching.RemoveAt( i );
			OnPlayerExit( player );
		}
	}

	protected virtual void OnPlayerEnter( UnicycleController player )
	{
		if ( EnterSound != null )
		{
			Sound.Play( EnterSound );
		}
	}

	protected virtual void OnPlayerExit( UnicycleController player )
	{
		if ( ExitSound != null )
		{
			Sound.Play( ExitSound );
		}
	}

	protected virtual void OnPlayerStay( UnicycleController player )
	{
	}

}
