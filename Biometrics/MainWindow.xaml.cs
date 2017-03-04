using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Biometrics.Classes;
using Biometrics.Views;
using Microsoft.Win32;

namespace Biometrics
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage origin;
        private Point pixelPoint;
        private byte[] pixels;

        public MainWindow()
        {
            InitializeComponent();
            origin = new BitmapImage(new Uri(@"pack://application:,,,/"
                                             + Assembly.GetExecutingAssembly().GetName().Name
                                             + ";component/"
                                             + "InitialImage/pic.png", UriKind.Absolute));
            var width = origin.PixelWidth.ToString();
            var height = origin.PixelHeight.ToString();


            ResolutionStatusBar.Text = width + " x " + height;
        }

        private void MenuExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuOpenImgFileClick(object sender, RoutedEventArgs e)
        {
            var op = new OpenFileDialog
            {
                Title = "Wybierz obraz",
                Filter = "Wszystkie wspierane obrazy|*.jpg;*.jpeg;*.bmp;*.png;*.gif;*.tiff;|" +
                         "Obraz PNG (*.png)|*.png|" + "Obraz JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "Obraz BMP (*.bmp)|*.bmp|" +
                         "Obraz GIF(*.gif)|*.gif|" + "Obraz TIFF(*.tiff)|*.tiff"
            };


            if (op.ShowDialog() != true) return;

            origin = new BitmapImage(new Uri(op.FileName));
            OriginalImage.Source = origin;
            ModifiedImage.Source = origin;
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
                MessageBox.Show("Wartości liczbowe w polach RGB muszą być z przedziału 0-255", "Uwaga",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }


        private void MouseClickedOnOriginalImage(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount != 2) return;
                var image = (Image) sender;
                var proportionheight = origin.PixelHeight / image.ActualHeight;
                var proportionwidth = origin.PixelWidth / image.ActualWidth;
                var point = e.GetPosition(OriginalImage);
                var x = point.X * proportionwidth;
                var y = point.Y * proportionheight;

                pixels = new byte[4];

                var bitmap = new CroppedBitmap(OriginalImage.Source as BitmapSource,
                    new Int32Rect((int) x, (int) y, 1, 1));

                try
                {
                    bitmap.CopyPixels(pixels, 4, 0);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }

                var aa = new RgbDialog(pixels[2], pixels[1], pixels[0], point);
                
                aa.Show();

                RLabel.Text = pixels[2].ToString(CultureInfo.CurrentCulture);
                GLabel.Text = pixels[1].ToString(CultureInfo.CurrentCulture);
                BLabel.Text = pixels[0].ToString(CultureInfo.CurrentCulture);
                pixelPoint = point;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void OpenImage()
        {
        }

        private static void SaveImageToFile(FrameworkElement img)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Zapisz obraz do pliku",
                Filter = "Obraz PNG (*.png)|*.png|" + "Obraz JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "Obraz BMP (*.bmp)|*.bmp|" +
                         "Obraz GIF(*.gif)|*.gif|" + "Obraz TIFF(*.tiff)|*.tiff"
            };
            if (dialog.ShowDialog() != true) return;
            var name = dialog.FileName;
            var type = dialog.FilterIndex;

            switch (type)
            {
                case 1:
                    SaveImage.SaveToPng(img, name);
                    break;
                case 2:
                    SaveImage.SaveToJpeg(img, name);
                    break;
                case 3:
                    SaveImage.SaveToBmp(img, name);
                    break;
                case 4:
                    SaveImage.SaveToGif(img, name);
                    break;
                case 5:
                    SaveImage.SaveToTiff(img, name);
                    break;
                default:
                    break;
            }
        }

        private void MenuSaveImgFile_OnClick(object sender, RoutedEventArgs e)
        {
            SaveImageToFile(ModifiedImage);
        }
    }
}