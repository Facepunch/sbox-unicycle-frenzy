
using Sandbox;
using System.ComponentModel.DataAnnotations;

[Library( "uf_trigger_surface", Description = "Overrides surface friction in an area" )]
//[Hammer.AutoApplyMaterial( "materials/editor/uf_trigger_fall.vmat" )]
[Display( Name = "Trigger Surface", GroupName = "Unicycle Frenzy", Description = "Overrides surface friction in an area." )]
internal partial class SurfaceTrigger : BaseTrigger
{

	[Net, Property]
	public SurfaceTypes SurfaceType { get; set; } = SurfaceTypes.Default;
	[Net, Property]
	public bool OverrideDefaultFriction { get; set; } = false;
	[Net, Property]
	public float FrictionOverride { get; set; } = 1f;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
		EnableTouchPersists = true;
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

		if ( other is not UnicyclePlayer pl ) return;
		if ( !pl.IsValid() ) return;

		pl.SurfaceFriction = GetSurfaceFriction();
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );

		if ( other is not UnicyclePlayer pl ) return;
		if ( !pl.IsValid() ) return;

		pl.SurfaceFriction = 1f;
	}

	private float GetSurfaceFriction()
	{
		if ( OverrideDefaultFriction ) 
			return FrictionOverride;

		return SurfaceType switch
		{
			SurfaceTypes.Mud => 10f,
			SurfaceTypes.Grass => 1.5f,
			SurfaceTypes.Sand => 20f,
			SurfaceTypes.Dirt => 1.5f,
			SurfaceTypes.Snow => 2f,
			SurfaceTypes.Ice => .5f,
			_ => 1f
		};
	}

	public enum SurfaceTypes
	{
		Default,
		Pavement,
		Mud,
		Snow,
		Ice,
		Grass,
		Dirt,
		Sand,
		Metal,
		Glass,
		Plastic
	}

}
