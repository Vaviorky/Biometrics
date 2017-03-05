using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Biometrics.Classes;
using Biometrics.Views;
using Microsoft.Win32;
using Image = System.Windows.Controls.Image;

namespace Biometrics
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static BitmapImage origin;
        private byte[] pixels;
        private string _path;

        public static Image ModifiedImgSingleton;

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
            _path = op.FileName;
            origin = new BitmapImage(new Uri(op.FileName));
            OriginalImage.Source = origin;
            ModifiedImage.Source = origin;
            ModifiedImgSingleton = ModifiedImage;
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
                    Debug.WriteLine(exc.StackTrace);
                }

                var changePixel = new RgbDialog(pixels[2], pixels[1], pixels[0], point, origin);
                changePixel.Show();
            }
            catch (Exception ee)
            {
                Debug.WriteLine(ee.StackTrace);
            }
        }

        private static void SaveImageToFile(BitmapSource img)
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
            var bitmap = ModifiedImgSingleton.Source as BitmapSource;
            SaveImageToFile(bitmap);
        }

    }
}