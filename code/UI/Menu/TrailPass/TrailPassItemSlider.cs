
using Sandbox;
using Sandbox.UI;

internal class TrailPassItemSlider : Panel
{

	public TrailPassItemSlider()
	{
		BuildItemList();
	}

	private void BuildItemList()
	{
		DeleteChildren();

		var items = TrailPass.Current?.Items;

		if ( items == null ) return;

		foreach ( var item in items )
		{
			var itemicon = new TrailPassItemIcon() { Item = item };
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
