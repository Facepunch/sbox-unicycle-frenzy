
using Sandbox;

internal partial class UnicyclePlayer
{

	private ModelEntity RagdollModel( ModelEntity modelEnt )
	{
		var ent = new ModelEntity();
		ent.Position = modelEnt.Position;
		ent.Rotation = modelEnt.Rotation;
		ent.Scale = modelEnt.Scale;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.SetModel( modelEnt.GetModelName() );
		ent.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		ent.CopyBonesFrom( modelEnt );
		ent.CopyBodyGroups( modelEnt );
		ent.CopyMaterialGroup( modelEnt );
		ent.TakeDecalsFrom( modelEnt );
		ent.CopyMaterialOverrides( modelEnt );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = modelEnt.RenderColor;
		ent.PhysicsGroup.Velocity = modelEnt.Velocity;

		foreach ( var child in modelEnt.Children )
		{
			if ( !child.Tags.Has( "clothes" ) ) continue;
			if ( child is not ModelEntity e ) continue;

			var model = e.GetModelName();

			var clothing = new ModelEntity();
			clothing.SetModel( model );
			clothing.SetParent( ent, true );
			clothing.RenderColor = e.RenderColor;
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
			clothing.CopyMaterialOverrides( e );
		}

		Juice.Scale( 1, 1.1f, 0f )
			.WithDelay( 7f )
			.WithTarget( ent )
			.WithDuration( .75f )
			.WithEasing( EasingType.EaseIn );

		ent.DeleteAsync( 10.0f );

		return ent;
	}

	[ClientRpc]
	private void RagdollOnClient()
	{
		if ( !Citizen.IsValid() || !Unicycle.IsValid() ) return;

		var corpse = RagdollModel( Citizen );

		if ( Game.LocalPawn is UnicyclePlayer pl && ( IsLocalPawn || pl.SpectateTarget == this ) )
		{
			pl.Corpse = corpse;
			// TODO: SCREENSHAKE
			//new Perlin( 2f, 2, 3 );
		}

		RagdollModel( Unicycle.Frame );
		RagdollModel( Unicycle.Wheel );
		RagdollModel( Unicycle.Seat );
	}

}

