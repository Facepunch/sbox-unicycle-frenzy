
using Sandbox;
using Sandbox.UI;

[UseTemplate]
internal class TrailPassItemSlider : Panel
{

	public TrailPassItemSlider()
	{
		BuildItemList();
	}

	private void BuildItemList()
	{
		DeleteChildren();

		var trailpass = TrailPass.Current;

		foreach ( var item in trailpass.Items )
		{
			var itemicon = new TrailPassItemIcon( item );
			itemicon.Parent = this;
		}
	}

	public override void OnMouseWheel( float value )
	{
		base.OnMouseWheel( value );

		ScrollVelocity.x = ScrollVelocity.y * 1.5f;
		ScrollVelocity.y = 0;
	}

	public void ScrollRight()
	{
		ScrollVelocity.x += 40f;
	}

	public void ScrollLeft()
	{
		ScrollVelocity.x -= 40f;
	}

}
