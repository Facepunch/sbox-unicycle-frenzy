namespace Editor;

internal class UnicycleFrenzyItemsWidget : BaseResourceEditor<UnicycleFrenzyItems>
{
	SerializedObject Object;

	public UnicycleFrenzyItemsWidget()
	{
		Layout = Layout.Column();
	}

	protected override void Initialize( Asset asset, UnicycleFrenzyItems resource )
	{
		Layout.Clear( true );

		Object = resource.GetSerialized();

		var sheet = new ControlSheet();
		sheet.AddObject( Object );
		Layout.Add( sheet );

		var ip = Layout.Add( new IconProperty() );

		ip.Scene.InstallClothing( resource );
		ip.Clothing = resource;
		ip.Asset = asset;
		ip.ContentMargins = 16;

		Object.OnPropertyChanged += ( p ) =>
		{
			NoteChanged( p );
			ip.Scene.InstallClothing( resource );
		};
	}

	public static void RenderAllIcons()
	{
		using var progress = Progress.Start( "Rendering Icons" );
		var token = Progress.GetCancel();
		var allClothes = AssetSystem.All.Where( x => x.AssetType.FileExtension == "clothing" ).ToArray();

		int i = 0;
		foreach ( var asset in allClothes )
		{
			Progress.Update( asset.Name, ++i, allClothes.Length );

			var resource = asset.LoadResource<UnicycleFrenzyItems>();
			RenderIcon( asset, resource );

			if ( token.IsCancellationRequested )
				return;
		}
	}

	private static void RenderIcon( Asset asset, UnicycleFrenzyItems resource )
	{
		// force an icon path
		var iconInfo = resource.Icon;

		var dir = System.IO.Path.GetDirectoryName( asset.RelativePath ) + "/";

		iconInfo.Path = System.IO.Path.Combine( dir, asset.Name + ".png" );
		resource.Icon = iconInfo;

		var Scene = new ClothingScene();
		Scene.UpdateLighting();
		Scene.InstallClothing( resource );
		Scene.UpdateCameraPosition();

		var pixelMap = new Pixmap( 256, 256 );
		Scene.Camera.RenderToPixmap( pixelMap );

		if ( asset.SaveToDisk( resource ) )
		{
			asset.Compile( false );
		}

		var root = asset.AbsolutePath[0..^(asset.RelativePath.Length)];
		pixelMap.SavePng( root + iconInfo.Path );
	}

	class IconProperty : Widget
	{
		public ClothingScene Scene;

		public UnicycleFrenzyItems Clothing;
		public Asset Asset { get; set; }

		NativeRenderingWidget CanvasWidget;

		public IconProperty() : base( null )
		{
			Layout = Layout.Row();

			Scene = new ClothingScene();
			Scene.UpdateLighting();

			CanvasWidget = new NativeRenderingWidget( this );
			CanvasWidget.FixedHeight = 256;
			CanvasWidget.FixedWidth = 256;

			Layout.Add( CanvasWidget );
			Layout.AddStretchCell();
		}

		public string IconPath { get; internal set; }

		protected override void OnPaint()
		{
			Scene.UpdateCameraPosition();

			CanvasWidget.Camera = Scene.Camera;
			//	CanvasWidget.RenderScene();

			Update();
		}

		protected override void OnMousePress( MouseEvent e )
		{
			base.OnMousePress( e );
			
			if ( e.RightMouseButton )
			{
				var allClothes = AssetSystem.All.Where( x => x.AssetType.FileExtension == "ufitem" );

				var menu = new Menu( this );
				menu.AddOption( $"Render Icon", null, () => RenderIcon( Asset, Clothing ) );
				menu.AddOption( $"Render All Icons ({allClothes.Count():n0})", null, () => UnicycleFrenzyItemsWidget.RenderAllIcons() );
				menu.OpenAt( Application.CursorPosition );
			}
		}
	}

	public class ClothingScene
	{
		public SceneWorld World;
		public SceneCamera Camera;

		public SceneModel Body;

		public SceneModel TargetModel;

		List<SceneObject> LightingObjects = new();

		public float Pitch = 15.0f;
		public float Yaw = 35.0f;
		public SlotMode Target = SlotMode.ModelBounds;

		public enum SlotMode
		{
			ModelBounds,
			Face
		}

		public ClothingScene()
		{
			World = new SceneWorld();
			Camera = new SceneCamera( "ClothingEditor" );

			Body = new SceneModel( World, "models/citizen/citizen.vmdl", Transform.Zero );
			Body.Rotation = Rotation.From( 0, 0, 0 );
			Body.Position = 0;
			Body.SetAnimParameter( "b_grounded", true );
			Body.SetAnimParameter( "aim_eyes", Vector3.Forward * 100.0f );
			Body.SetAnimParameter( "aim_head", Vector3.Forward * 100.0f );
			Body.SetAnimParameter( "aim_body", Vector3.Forward * 100.0f );
			Body.SetAnimParameter( "aim_body_weight", 1.0f );
			Body.Update( 1 );
			Body.RenderingEnabled = false;

			Camera.World = World;
			Camera.BackgroundColor = new Color( 0.1f, 0.1f, 0.1f, 0.0f );
			Camera.AmbientLightColor = Color.Gray * 0.1f;

			TargetModel = Body;
		}

		SceneModel clothingModels;

		public void InstallClothing( UnicycleFrenzyItems clothing )
		{

			clothingModels = new SceneModel( World, clothing.ItemModel, Transform.Zero );
			TargetModel = clothingModels;
		}

		public void UpdateLighting()
		{
			foreach ( var light in LightingObjects )
			{
				light.Delete();
			}
			LightingObjects.Clear();

			var sun = new SceneDirectionalLight( World, Rotation.From( 45, -180, 0 ), Color.White * 0.5f + Color.Cyan * 0.05f )
			{
				ShadowsEnabled = true,
				SkyColor = Color.White * 0.05f + Color.Cyan * 0.025f
			};

			LightingObjects.Add( sun );

			LightingObjects.Add( new SceneCubemap( World, Texture.Load( "textures/cubemaps/default.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) ) );
			LightingObjects.Add( new SceneLight( World, new Vector3( -100, -10, 60 ), 200, new Color( 0.8f, 1, 1 ) * 1.3f ) { ShadowTextureResolution = 512 } );
			LightingObjects.Add( new SceneLight( World, new Vector3( -100, 150, 60 ), 400, new Color( 1, 0.9f, 0.6f ) * 16.0f ) { ShadowTextureResolution = 512 } );
			LightingObjects.Add( new SceneLight( World, new Vector3( 200, 50, 500 ), 1200, new Color( 1, 0.9f, 0.85f ) * 20.0f ) { ShadowTextureResolution = 512 } );
		}

		public void UpdateCameraPosition()
		{
			if ( TargetModel == null )
				return;

			Camera.FieldOfView = 5;
			Camera.ZFar = 10000;
			Camera.ZNear = 10;

			Body.Update( RealTime.Delta );
			TargetModel.Update( RealTime.Delta );
			TargetModel.Transform = TargetModel.Transform.WithScale( 1f );

			var bounds = TargetModel.Bounds;

			if ( Target == SlotMode.ModelBounds )
			{
				bounds = TargetModel.Model.RenderBounds;
			}
			else if ( Target == SlotMode.Face )
			{
				var headBone = TargetModel.GetBoneWorldTransform( "head" );
				headBone.Position += Vector3.Up * 6;
				bounds = new BBox( headBone.Position - 7, headBone.Position + 7 );
			}

			var lookAngle = new Angles( Pitch, 180 - Yaw, 0 );
			var forward = lookAngle.Forward;
			var distance = MathX.SphereCameraDistance( bounds.Size.Length * 0.5f, Camera.FieldOfView );

			Camera.Position = bounds.Center - forward * distance;
			Camera.Rotation = Rotation.From( lookAngle );
		}
	}
}
