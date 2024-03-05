using Editor;
using System.IO;

namespace Sandbox
{
	[EditorTool]
	[Title( "Thumbnail Creator")]
	[Icon("insert_photo")]
    public class ThumbnailTool : EditorTool
    {
		private SceneCamera _thumbnailCam;

		public override void OnEnabled()
		{
			var window = new WidgetWindow( SceneOverlay, "Thumbnail Creator" );
			window.Layout = Layout.Column();
			window.Layout.Margin = 16;

			var button = new Button( "Create Thumbnail" );
			button.Pressed = CreateThumbnail;

			window.Layout.Add( button );
			AddOverlay( window, TextFlag.RightTop, 10 );
		}

		public override void OnUpdate()
		{
			using ( Gizmo.Scope( "thumbnail tool" ) )
			{
				_thumbnailCam = Gizmo.Camera;
			}
		}

		private void CreateThumbnail()
		{
			using var cam = new SceneCamera( "Thumbnail Camera" );

			cam.World = Scene.SceneWorld;
			cam.Position = _thumbnailCam.Position;
			cam.Rotation = _thumbnailCam.Rotation;
			cam.ZNear = _thumbnailCam.ZNear;
			cam.ZFar = _thumbnailCam.ZFar;
			cam.FieldOfView = _thumbnailCam.FieldOfView;
			cam.BackgroundColor = Color.Transparent;
			var pix = new Pixmap( 512, 512 );
			cam.RenderToPixmap( pix );
			pix.SavePng( GetThumbnailPath() );
		}

		private string GetThumbnailPath()
		{
			var scenePath = FileSystem.Mounted.GetFullPath( Scene.Source.ResourcePath );
			var directory = Path.GetDirectoryName( scenePath );
			var thumbnailFileName = $"{Scene.Source.ResourceName}.png";
			return Path.Combine( directory, thumbnailFileName );
		}
	}
}
