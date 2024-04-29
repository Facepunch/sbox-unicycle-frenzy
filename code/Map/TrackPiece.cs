using Sandbox;

public sealed class TrackPiece : Component
{
	protected override void DrawGizmos()
	{
		base.DrawGizmos();
		var height = 100f;

		using ( Gizmo.Scope( "arrow" ) )
		{
			var bounds = Components.Get<ModelRenderer>().Bounds;
			Gizmo.Transform = new Transform( Transform.Position, Rotation.From( 0, 0, 0 ) );
			Gizmo.Draw.Arrow( Vector3.Zero + Vector3.Up * height, Vector3.Zero + Transform.Rotation.Forward * 100 + Vector3.Up * height );
		}

	}
}
