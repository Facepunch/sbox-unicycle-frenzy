
using Sandbox;
using System.ComponentModel.DataAnnotations;

[Library( "uf_trigger_tutorial", Description = "Enable tutorial features in this trigger" )]
//[Hammer.AutoApplyMaterial( "materials/editor/uf_trigger_fall.vmat" )]
[Display( Name = "Trigger Tutorial", GroupName = "Unicycle Frenzy", Description = "Enable tutorial features in this trigger." )]
internal partial class TutorialTrigger : BaseTrigger
{

	[Property]
	public InputActions DisplayBind { get; set; }
	[Property]
	public bool PerfectPedalGlow { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		EnableTouchPersists = true;
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( other is not UnicyclePlayer pl ) return;

		pl.PerfectPedalGlow = PerfectPedalGlow;
		pl.DisplayedAction = DisplayBind;
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not UnicyclePlayer pl ) return;
		
		pl.PerfectPedalGlow = false;
		pl.DisplayedAction = InputActions.None;
	}

}
