using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for RgbDialog.xaml
    /// </summary>
    public partial class RgbDialog : Window
    {
        private readonly Image _modifiedImage;
        private readonly byte _r, _g, _b;
        private readonly WriteableBitmap _writeableBitmap;
        private Point _mousePoint;
        private readonly BitmapImage _origin;
        private PixelFormat _pf = PixelFormats.Rgb24;

        public RgbDialog()
        {
            InitializeComponent();
        }

        public RgbDialog(byte r, byte g, byte b, Point mousePoint, BitmapImage origin)
        {
            InitializeComponent();

            Left = mousePoint.X;
            Top = mousePoint.Y;
            _r = r;
            _g = g;
            _b = b;
            _mousePoint = mousePoint;
            _origin = origin;

            RectangleColor.Fill = new SolidColorBrush(Color.FromRgb(_r, _g, _b));
            RLabel.Text = _r.ToString();
            GLabel.Text = _g.ToString();
            BLabel.Text = _b.ToString();
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

            var d = new DataObject(DataFormats.Bitmap, MainWindow.ModifiedImgSingleton.Source, true);
            var bitmap = d.GetData("System.Drawing.Bitmap") as Bitmap;

            var c = System.Drawing.Color.FromArgb(255, R, G, B);


            var proportionheight = _origin.PixelHeight / MainWindow.ModifiedImgSingleton.ActualHeight;
            var proportionwidth = _origin.PixelWidth / MainWindow.ModifiedImgSingleton.ActualWidth;

            var x = (int) (_mousePoint.X * proportionwidth);
            var y = (int) (_mousePoint.Y * proportionheight);

            bitmap.SetPixel(x, y, c);

            var hBitmap = bitmap.GetHbitmap();

            var bitmapsource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            MainWindow.ModifiedImgSingleton.Source = bitmapsource;
        }
    }
}