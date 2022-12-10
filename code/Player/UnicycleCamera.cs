
using Sandbox;
using System;
using System.Collections.Generic;

internal class UnicycleCamera
{
	public Vector3 Position { get; set; }
	public Rotation Rotation { get; set; }
	public float FieldOfView { get; set; }
	
	private List<UfProp> viewblockers = new();
	private List<Checkpoint> cpviewblockers = new();
	private Angles ViewAngles;

	public void Update()
	{
		var pawn = Game.LocalPawn as UnicyclePlayer;

		if ( pawn == null ) return;
		if ( pawn.SpectateTarget.IsValid() ) pawn = pawn.SpectateTarget;

		var dir = (Position - pawn.Position).Normal;
		var pos = pawn.Position + dir * 15f;
		Sound.Listener = new Transform( pos, Rotation );

		ClearViewBlockers();
		UpdateViewBlockers( pawn );

		var viewangles = pawn.ViewAngles;

		// hack in sensitivity boost until we get sens slider for controllers
		if ( Input.UsingController )
		{
			var delta = Input.GetAnalog( InputAnalog.Look );
			viewangles += new Vector3( -delta.y * 6f, -delta.x * 6f, 0f );
		}

		viewangles.pitch = Math.Clamp( viewangles.pitch, -35f, 65f );

		var targetRot = Rotation.From( viewangles );
		var center = pawn.Position + Vector3.Up * 80;
		var distance = 150.0f * pawn.Scale;
		var targetPos = center + targetRot.Forward * -distance;

		var tr = Trace.Ray( center, targetPos )
			.Ignore( pawn )
			.Radius( 8 )
			.Run();

		var endpos = tr.EndPosition;

		if ( tr.Entity is UfProp ufp && ufp.NoCameraCollide || tr.Entity is Checkpoint ufcp && ufcp.NoCameraCollide )
			endpos = targetPos;

		Position = endpos;
		Rotation = targetRot;

		// controller constantly tries to center itself
		if ( Input.UsingController && pawn.Velocity.WithZ( 0 ).Length > 35 )
		{
			var defaultPosition = pawn.TargetForward.Angles().WithPitch( 30 );
			Rotation = Rotation.Lerp( Rotation, Rotation.From( defaultPosition ), Time.Delta );
		}

		var spd = pawn.Velocity.WithZ( 0 ).Length / 350f;
		var fov = 62f.LerpTo( 82f, spd );

		FieldOfView = FieldOfView.LerpTo( fov, Time.Delta );

		Camera.Position = Position;
		Camera.Rotation = Rotation;
		Camera.FirstPersonViewer = null;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( FieldOfView );
	}

	public void BuildInput()
	{
		if ( Game.LocalPawn is not UnicyclePlayer pl )
			return;
		
		var diff = Rotation.Difference( Rotation.From( 0, pl.ViewAngles.yaw, 0 ), Rotation.From( 0, pl.Rotation.Yaw(), 0 ) );
		if( diff.Angle() > 170 )
		{
			Input.AnalogLook = default;
		}

		if ( !Input.UsingController ) return;

		// controllers get special handling so update ViewAngles here
		pl.ViewAngles = Rotation.Angles();
		
	}

	public void Activated()
	{
		FieldOfView = 85;
	}

	public void Deactivated()
	{
		ClearViewBlockers();
	}

	private void ClearViewBlockers()
	{
		foreach ( var ent in viewblockers )
		{
			ent.BlockingView = false;
		}
		foreach ( var ent in cpviewblockers )
		{
			ent.BlockingView = false;
		}
		viewblockers.Clear();
	}

	private void UpdateViewBlockers( UnicyclePlayer pawn )
	{
		var traces = Trace.Sphere( 3f, Camera.Position, pawn.Position + Vector3.Up * 16 ).RunAll();

		if ( traces == null ) return;

		foreach ( var tr in traces )
		{
			if ( tr.Entity is not UfProp prop || tr.Entity is not Checkpoint cp ) continue;
			prop.BlockingView = true;
			cp.BlockingView = true;
			viewblockers.Add( prop );
			cpviewblockers.Add( cp );
		}
	}

}
