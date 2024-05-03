using Sandbox;

public sealed class PlayerGizmo : Component
{
	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		Gizmo.Draw.Color = Color.White.WithAlpha( 0.5f );
		Gizmo.Draw.Model( Model.Load( "models/unicycles/gizmo_unicycle_basic.vmdl" ) );
	}
	protected override void OnUpdate()
	{

	}
}
