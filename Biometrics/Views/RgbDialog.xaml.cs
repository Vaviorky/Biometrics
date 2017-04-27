using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for RgbDialog.xaml
    /// </summary>
    public partial class RgbDialog
    {
        private readonly BitmapSource _origin;
        private Point _mousePoint;
        private byte[] _pixels;
        public RgbDialog()
        {
            InitializeComponent();
        }

        public RgbDialog(byte []pixels, Point mousePoint, BitmapSource origin)
        {
            InitializeComponent();

            _pixels = pixels;

            Left = mousePoint.X;
            Top = mousePoint.Y;
            var r = pixels[2];
            var g = pixels[1];
            var b = pixels[0];
            _mousePoint = mousePoint;
            _origin = origin;

            RectangleColor.Fill = new SolidColorBrush(Color.FromRgb(r, g, b));
            RLabel.Text = r.ToString();
            GLabel.Text = g.ToString();
            BLabel.Text = b.ToString();
        }

        private void RgbValueChanged(object sender, RoutedEventArgs e)
        {
            var r = RLabel.Text;
            var g = GLabel.Text;
            var b = BLabel.Text;

            if (r == "" || g == "" || b == "")
            {
                MessageBox.Show("Wartości w polach RGB nie mogą być puste", "Uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int R, G, B;
            var rsuccess = int.TryParse(r, out R);
            var gsuccess = int.TryParse(g, out G);
            var bsuccess = int.TryParse(b, out B);
            if (!rsuccess || !gsuccess || !bsuccess)
            {
                MessageBox.Show("Wartości w polach RGB muszą być liczbowe", "Uwaga", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (R < 0 || R > 255 || G < 0 || G > 255 || B < 0 || B > 255)
            {
                MessageBox.Show("Wartości liczbowe w polach RGB muszą być z przedziału 0-255", "Uwaga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Close();


            BitmapSource image;
            var wbm = MainWindow.ModifiedImgSingleton.Source as WriteableBitmap;
            if (wbm != null)
                image = ConvertWriteableBitmapToBitmapImage(wbm);
            else
                image =  (BitmapSource) MainWindow.ModifiedImgSingleton.Source;
            var width = (int) image.Width;
            var height = (int) image.Height;
            var bitmap = new WriteableBitmap(image);
            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
            var arraySize = stride * height;
            var pixels = new byte[arraySize];
            bitmap.CopyPixels(pixels, stride, 0);
            var proportionheight = _origin.PixelHeight / MainWindow.ModifiedImgSingleton.ActualHeight;
            var proportionwidth = _origin.PixelWidth / MainWindow.ModifiedImgSingleton.ActualWidth;

            var x = (int) (_mousePoint.X * proportionwidth);
            var y = (int) (_mousePoint.Y * proportionheight);
            var j = 4 * x + y * stride;

            pixels[j] = (byte) B;
            pixels[j + 1] = (byte) G;
            pixels[j + 2] = (byte) R;
            pixels[j + 3] = 255;


            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);
            MainWindow.ModifiedImgSingleton.Source = bitmap;

            MainWindow.ImageModificationsList.Add(bitmap);
        }

        private static BitmapImage ConvertWriteableBitmapToBitmapImage(BitmapSource wbm)
        {
            var bmImage = new BitmapImage();
            using (var stream = new MemoryStream())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
    }
}