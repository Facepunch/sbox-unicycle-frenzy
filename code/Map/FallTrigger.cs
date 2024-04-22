using Sandbox;

[EditorHandle( "textures/sprays/spray_bomb.png" )]
public sealed class FallTrigger : Component, Component.ITriggerListener
{
	[Property] public BBox Bounds { get; set; } = BBox.FromPositionAndSize( 0, 64 );
	BoxCollider Box;
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
		Tags.Add("trigger");
	}
	void ITriggerListener.OnTriggerEnter( Collider other )
	{
		var ply = other.Components.Get<UnicycleController>();
		if ( ply == null ) return;
		ply.ForceFall = true;
	}
}
