using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private static BitmapImage _originalImgBitmap;

        public static Image ModifiedImgSingleton;
        private MatrixTransform _originalMatrix, _modifiedMatrix;
        private byte[] pixels;
        private Point start, origin;


        public MainWindow()
        {
            InitializeComponent();
            _originalImgBitmap = new BitmapImage(new Uri(@"pack://application:,,,/"
                                                         + Assembly.GetExecutingAssembly().GetName().Name
                                                         + ";component/"
                                                         + "InitialImage/pic.png", UriKind.Absolute));
            var width = _originalImgBitmap.PixelWidth.ToString();
            var height = _originalImgBitmap.PixelHeight.ToString();
            ResolutionStatusBar.Text = width + " x " + height;
            ModifiedImgSingleton = ModifiedImage;

            _originalMatrix = (MatrixTransform) OriginalImage.RenderTransform;
            _modifiedMatrix = (MatrixTransform) ModifiedImage.RenderTransform;
        }

        private void MenuExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region Open and Save Image

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
            _originalImgBitmap = new BitmapImage(new Uri(op.FileName));
            OriginalImage.Source = _originalImgBitmap;
            ModifiedImage.Source = _originalImgBitmap;
            ModifiedImgSingleton = ModifiedImage;

            ResetPositionAndZoomOfImage();

            _originalMatrix = (MatrixTransform) OriginalImage.RenderTransform;
            _modifiedMatrix = (MatrixTransform) ModifiedImage.RenderTransform;
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

        #endregion

        #region Zooming and Panning

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var point = e.GetPosition(Equals(sender, OriginalImage) ? OriginalImage : ModifiedImage);

            var stOriginal = (MatrixTransform) OriginalImage.RenderTransform;
            var stCopy = (MatrixTransform) ModifiedImage.RenderTransform;
            var zoom = e.Delta >= 0 ? 1.1 : 1.0 / 1.1;

            var matrixOriginal = stOriginal.Matrix;
            var matrixCopy = stCopy.Matrix;

            matrixOriginal.ScaleAtPrepend(zoom, zoom, point.X, point.Y);
            matrixCopy.ScaleAtPrepend(zoom, zoom, point.X, point.Y);

            OriginalImage.RenderTransform = new MatrixTransform(matrixOriginal);
            ModifiedImage.RenderTransform = new MatrixTransform(matrixCopy);
        }

        private void Image_MouseClicked(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, OriginalImage))
            {
                if (OriginalImage.IsMouseCaptured) return;

                OriginalImage.CaptureMouse();
                start = e.GetPosition(OriginalBorder);
            }

            if (Equals(sender, ModifiedImage))
            {
                if (ModifiedImage.IsMouseCaptured) return;

                ModifiedImage.CaptureMouse();
                start = e.GetPosition(ModifiedBorder);
            }

            origin.X = OriginalImage.RenderTransform.Value.OffsetX;
            origin.Y = OriginalImage.RenderTransform.Value.OffsetY;


            if (Equals(sender, OriginalImage))
                try
                {
                    if (e.ClickCount != 2) return;
                    var image = (Image) sender;
                    var proportionheight = _originalImgBitmap.PixelHeight / image.ActualHeight;
                    var proportionwidth = _originalImgBitmap.PixelWidth / image.ActualWidth;
                    var point = e.GetPosition(OriginalImage);
                    var x = point.X * proportionwidth;
                    var y = point.Y * proportionheight;

                    pixels = new byte[4];

                    var bitmap = new CroppedBitmap(_originalImgBitmap,
                        new Int32Rect((int) x, (int) y, 1, 1));

                    try
                    {
                        bitmap.CopyPixels(pixels, 4, 0);
                    }
                    catch (Exception exc)
                    {
                        Debug.WriteLine(exc.StackTrace);
                    }

                    var changePixel = new RgbDialog(pixels[2], pixels[1], pixels[0], point, _originalImgBitmap);
                    changePixel.Show();
                }
                catch (Exception ee)
                {
                    Debug.WriteLine(ee.StackTrace);
                }
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            Point p;

            if (Equals(sender, OriginalImage))
            {
                if (!OriginalImage.IsMouseCaptured) return;
                p = e.MouseDevice.GetPosition(OriginalBorder);
            }
            else
            {
                if (!ModifiedImage.IsMouseCaptured) return;
                p = e.MouseDevice.GetPosition(ModifiedBorder);
            }


            var originalMatrix = OriginalImage.RenderTransform.Value;
            originalMatrix.OffsetX = origin.X + (p.X - start.X);
            originalMatrix.OffsetY = origin.Y + (p.Y - start.Y);

            var modifiedMatrix = ModifiedImage.RenderTransform.Value;
            modifiedMatrix.OffsetX = origin.X + (p.X - start.X);
            modifiedMatrix.OffsetY = origin.Y + (p.Y - start.Y);


            OriginalImage.RenderTransform = new MatrixTransform(originalMatrix);
            ModifiedImage.RenderTransform = new MatrixTransform(modifiedMatrix);
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, OriginalImage))
                OriginalImage.ReleaseMouseCapture();

            if (Equals(sender, ModifiedImage))
                ModifiedImage.ReleaseMouseCapture();
        }

        private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ResetPositionAndZoomOfImage();
        }

        private void ResetPositionAndZoomOfImage()
        {
            var matrixOriginal = _originalMatrix.Matrix;
            var matrixCopy = _modifiedMatrix.Matrix;

            matrixOriginal.ScaleAtPrepend(1.0, 1.0, 1.0, 1.0);
            matrixCopy.ScaleAtPrepend(1.0, 1.0, 1.0, 1.0);

            OriginalImage.RenderTransform = new MatrixTransform(matrixOriginal);
            ModifiedImage.RenderTransform = new MatrixTransform(matrixCopy);
        }

        #endregion
    }
}