using Sandbox;

[EditorHandle( "textures/sprays/spray_bomb.png" )]
public sealed class FallTrigger : Component
{
	[Property] public BBox Bounds { get; set; } = BBox.FromPositionAndSize( 0, 64 );
	BoxCollider Box;
	List<UnicycleController> LastTouching = new();
	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Draw.Color = Color.Red.WithAlpha( 0.25f );
		Gizmo.Draw.SolidBox( Bounds );

		Gizmo.Draw.Color = Color.Red;
		Gizmo.Draw.LineBBox( Bounds );

		if ( !Gizmo.IsSelected ) return;

		if ( Gizmo.Control.BoundingBox( "bbox", Bounds, out var newBounds ) )
		{
			Bounds = newBounds;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		Box = Components.Create<BoxCollider>();
		Box.Scale = Bounds.Size;
		Box.Center = Bounds.Center;
		Box.IsTrigger = true;
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

			player.Fall();
		}

		for ( int i = LastTouching.Count - 1; i >= 0; i-- )
		{
			var player = LastTouching[i];
			if ( touchingThisFrame.Contains( player ) ) continue;
			LastTouching.RemoveAt( i );
		}
	}
}
