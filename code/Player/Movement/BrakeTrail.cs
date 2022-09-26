
using Sandbox;

internal partial class BrakeTrail : Entity
{

	private TimeSince TimeSinceCreated;

	public Particles Particles { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		TimeSinceCreated = 0;
		Transmit = TransmitType.Always;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		Particles = Particles.Create( "particles/trails/default_trail.vpcf", this );
		Particles.SetPosition( 6, new Vector3( 15, 0, 0 ) );
		Particles.SetPosition( 8, 1 );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Particles?.Destroy();
		Particles = null;
	}

	public override void SetParent( Entity entity, string attachmentOrBoneName = null, Transform? transform = null )
	{
		base.SetParent( entity, attachmentOrBoneName, transform );

		if ( entity != null )
		{
			LocalPosition = 0;
			LocalRotation = Rotation.Identity;
			LocalScale = 1;
		}
	}

	[Event.Tick.Server]
	private void OnTick()
	{
		if( TimeSinceCreated> 30 )
		{
			this.Delete();
		}
	}

}
