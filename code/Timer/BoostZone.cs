
internal class BoostZone : BaseZone
{

	protected override Color ZoneColor => Color.Yellow;
	public override bool IsCheckPoint { get; set; } = false;
	[Property] public float BoostAmount { get; set; } = 1000;
	protected override void OnPlayerEnter( UnicycleController player )
	{
		base.OnPlayerEnter( player );


		player.Velocity = player.Velocity + player.Velocity.Normal * BoostAmount;
	}

}
