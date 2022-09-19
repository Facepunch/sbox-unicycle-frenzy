
using Sandbox.UI;

[UseTemplate]
internal class ProgressBar : Panel
{

	public Panel Bar { get; set; }
	public Label Title { get; set; }

	public void Set( int current, int max )
	{
		Title.Text = $"{current}/{max}";
		Bar.Style.Width = Length.Fraction( current / (float)max );
	}

}
