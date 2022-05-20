
using Sandbox;
using SandboxEditor;
using System.ComponentModel.DataAnnotations;

[Library( "uf_trigger_reset_progress", Description = "Resets the player's progress by clearing checkpoints" )]
[AutoApplyMaterial("materials/editor/uf_trigger_reset_progress.vmat")]
[Display( Name = "Reset Progress", GroupName = "Unicycle Frenzy", Description = "Resets the player's progress by clearing checkpoints" )]
[HammerEntity]
internal partial class ResetProgressTrigger : BaseTrigger
{

	public override void StartTouch( Entity other )
	{
		base.StartTouch( other );

		if ( !other.IsServer ) return;
		if ( other is not UnicyclePlayer pl ) return;

		pl.ClearCheckpoints();
	}

}
