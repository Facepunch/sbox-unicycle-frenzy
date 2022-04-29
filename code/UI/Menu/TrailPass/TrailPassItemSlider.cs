
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

}
