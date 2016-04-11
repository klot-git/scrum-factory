using System.IO;
using System.Windows.Media.Imaging;



namespace ScrumFactory.Windows.Properties {

    public class ImageResources {

        private static BitmapSource ToBitmapSource(System.Drawing.Bitmap bitmap) {
            System.IO.MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;

            var result = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            result.Freeze();
            return result;

        }

        public static BitmapSource DONE_PANEL_TITLE {
            get {
                return ToBitmapSource(Properties.Resources.done_panel_img);
            }
        }

        public static BitmapSource WORKING_PANEL_TITLE {
            get {
                return ToBitmapSource(Properties.Resources.working_panel_img);
            }
        }

        public static BitmapSource TODO_PANEL_TITLE {
            get {
                return ToBitmapSource(Properties.Resources.todo_panel_img);
            }
        }

    }



}
