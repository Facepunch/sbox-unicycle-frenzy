
using Sandbox;
using Sandbox.UI;

internal class RadialFill : Panel
{

	public float BorderWidth { get; set; } = 20;
	public float EdgeGap { get; set; } = 2;
	public int Points { get; set; } = 64;
	public float FillStart { get; set; } = .5f;
	public float FillAmount { get; set; } = 0.37f;
	public Color TrackColor { get; set; } = Color.White;
	public Color FillColor { get; set; } = Color.Blue;

	public override void DrawBackground( ref RenderState state )
	{
		base.DrawBackground( ref state );

		var center = Box.Rect.Center;
		var radius = Box.Rect.Width * .5f;
		var draw = Render.Draw2D;
		draw.Reset();

		draw.Color = TrackColor;
		draw.CircleEx( center, radius, radius - BorderWidth, Points );

		draw.Color = FillColor;
		draw.CircleEx( center, radius - EdgeGap, radius - BorderWidth + EdgeGap, Points, FillStart * 360, (FillStart + FillAmount) * 360 );
	}

}
