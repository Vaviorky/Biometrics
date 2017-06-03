using System;
using System.Collections.Generic;
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
        private static BitmapSource _originalImgBitmap;
        private static BitmapSource originalBitmapSource;

        public static Image ModifiedImgSingleton;

        private MatrixTransform _originalMatrix, _modifiedMatrix;
        private byte[] pixels;
        private Point start, origin;

        public MainWindow()
        {
            InitializeComponent();
            ImageModificationsList = new List<ImageSource>();
            _originalImgBitmap = new BitmapImage(new Uri(@"pack://application:,,,/"
                                                         + Assembly.GetExecutingAssembly().GetName().Name
                                                         + ";component/"
                                                         + "InitialImage/finger.png", UriKind.Absolute));
            var width = _originalImgBitmap.PixelWidth.ToString();
            var height = _originalImgBitmap.PixelHeight.ToString();
            ResolutionStatusBar.Text = width + " x " + height;
            ModifiedImgSingleton = ModifiedImage;

            ResolutionX.Text = width;
            ResolutionY.Text = height;
            SaveImage.X = int.Parse(ResolutionX.Text);
            SaveImage.Y = int.Parse(ResolutionY.Text);
            _originalMatrix = (MatrixTransform)OriginalImage.RenderTransform;
            _modifiedMatrix = (MatrixTransform)ModifiedImage.RenderTransform;

            originalBitmapSource = (BitmapSource)OriginalImage.Source;

            //calculate initial histogram values
            HistogramTools.CalculateHistograms();
        }

        public static List<ImageSource> ImageModificationsList { get; set; }


        private void MenuExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenHistogram(object sender, RoutedEventArgs e)
        {
            //open window where user can see image and it's histograms for r/g/b channels and average channel
            var histogramwindow = new HistogramWindow((BitmapSource)ModifiedImgSingleton.Source);
            histogramwindow.Show();
        }

        #region filters

        private void FilterItem_MaskOnClick(object sender, RoutedEventArgs e)
        {
            var manualMask = new ManualMask();
            manualMask.Show();
        }

        private void FilterItem_BlurOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.Blur, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_PrewittHorizontalOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.PrewittHorizontal, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_PrewittVerticalOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.PrewittVertical, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_SobelHorizontalOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.SobelHorizontal, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_SobelVerticalOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.SobelVertical, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_Laplace4OnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.LaplaceMiddle4, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_Laplace8OnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.LaplaceMiddle8, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_DetectCornersEast(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.EastCorner, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_DetectCornersWest(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.WestCorner, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_DetectCornersNorthWest(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.NorthWestCorner, 3);
            HistogramTools.CalculateHistograms();
        }

        private void FilterItem_DetectCornersSouthEast(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Mask.PerformMask(MaskTables.SouthEastCorner, 3);
            HistogramTools.CalculateHistograms();
        }

        private async void FilterItem_KuwaharMask3(object sender, RoutedEventArgs e)
        {
            ProgressBarKuhara.Value = 0;
            ProgressBarKuhara.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => { ProgressBarKuhara.Value = value; });
            ModifiedImgSingleton.Source = await Mask.PerformMask_Kuwahar(3, progress);

            ProgressBarKuhara.Visibility = Visibility.Hidden;
            HistogramTools.CalculateHistograms();
        }

        private async void FilterItem_KuwaharMask5(object sender, RoutedEventArgs e)
        {
            ProgressBarKuhara.Value = 0;
            ProgressBarKuhara.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => { ProgressBarKuhara.Value = value; });
            ModifiedImgSingleton.Source = await Mask.PerformMask_Kuwahar(5, progress);

            ProgressBarKuhara.Visibility = Visibility.Hidden;
            HistogramTools.CalculateHistograms();
        }

        private async void FilterItem_Median3(object sender, RoutedEventArgs e)
        {
            ProgressBarKuhara.Value = 0;
            ProgressBarKuhara.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => { ProgressBarKuhara.Value = value; });
            ModifiedImgSingleton.Source = await Mask.PerformMask_Median(3, progress);

            ProgressBarKuhara.Visibility = Visibility.Hidden;
            HistogramTools.CalculateHistograms();
        }

        private async void FilterItem_Median5(object sender, RoutedEventArgs e)
        {
            ProgressBarKuhara.Value = 0;
            ProgressBarKuhara.Visibility = Visibility.Visible;
            var progress = new Progress<int>(value => { ProgressBarKuhara.Value = value; });
            ModifiedImgSingleton.Source = await Mask.PerformMask_Median(5, progress);

            ProgressBarKuhara.Visibility = Visibility.Hidden;
            HistogramTools.CalculateHistograms();
        }

        #endregion

        #region ModificationImageOptionsClickedEvents

        #region Histograms and Brightness

        private void HistogramEqualizationOnClick(object sender, RoutedEventArgs e)
        {
            //get dystrybuanta values
            var dystR = HistogramTools.GetDystrybuanta(HistogramTools.HistogramR);
            var dystG = HistogramTools.GetDystrybuanta(HistogramTools.HistogramG);
            var dystB = HistogramTools.GetDystrybuanta(HistogramTools.HistogramB);

            //get lookup table values after passing dystrybuanta for each channel and length of each channel table
            var lutR = HistogramTools.GetLutEqualization(dystR, HistogramTools.HistogramR.Length);
            var lutG = HistogramTools.GetLutEqualization(dystG, HistogramTools.HistogramG.Length);
            var lutB = HistogramTools.GetLutEqualization(dystB, HistogramTools.HistogramB.Length);

            //equalize histogram
            ModifiedImgSingleton.Source = HistogramTools.EqualizeHistogram(lutR, lutG, lutB);
            //update histogram
            HistogramTools.CalculateHistograms();
        }

        private void HistogramStretchingOnClick(object sender, RoutedEventArgs e)
        {
            //open window where user can set a and b values for Histogram Stretching
            var histogramStretching = new HistogramStretching();
            histogramStretching.Show();
        }

        private void ImageBrighteningOnClick(object sender, RoutedEventArgs e)
        {
            var lut = new int[256];
            var max = 0;

            //loop while value of averagedhistogram is equal to zero, if not, assign this 
            //position of nonzero value to new variable and break
            for (var i = 255; i >= 0; i--)
                if (HistogramTools.HistogramU[i] > 0)
                {
                    max = i;
                    break;
                }


            for (var i = 0; i < 256; i++)
            {
                //using logarithm function to obscure image
                var licznik = Math.Log(1 + i);
                var mianownik = Math.Log(1 + max);
                lut[i] = (int)Math.Round(255.0 * (licznik / mianownik), 0, MidpointRounding.AwayFromZero);

                //if value extends range [0..255], set margin value
                if (lut[i] > 255)
                    lut[i] = 255;
                if (lut[i] < 0)
                    lut[i] = 0;
            }

            var bitmap = new WriteableBitmap((BitmapSource)ModifiedImgSingleton.Source);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;

            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //copy all data about pixels values into 1-dimentional array
                var r = pixels[j + 2];
                var g = pixels[j + 1];
                var b = pixels[j];

                //assign new values from lut at r/g/b positions
                pixels[j + 2] = (byte)lut[r];
                pixels[j + 1] = (byte)lut[g];
                pixels[j] = (byte)lut[b];

                j += 4;
            }

            //overwrite pixels table
            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            //set updated image
            ModifiedImgSingleton.Source = bitmap;
            //recalculate histograms for new image
            HistogramTools.CalculateHistograms();
        }

        private void ImageObscuringOnClick(object sender, RoutedEventArgs e)
        {
            var lut = new int[256];
            var max = 0;

            //loop while value of averagedhistogram is equal to zero, if not, assign this 
            //position of nonzero value to new variable and break
            for (var i = 255; i >= 0; i--)
                if (HistogramTools.HistogramU[i] > 0)
                {
                    max = i;
                    break;
                }


            for (var i = 0; i < 256; i++)
            {
                //using power function to obscure image
                lut[i] = (int)Math.Round(255.0 * Math.Pow((double)i / max, 2.0), 0, MidpointRounding.AwayFromZero);

                if (lut[i] > 255)
                    lut[i] = 255;
                if (lut[i] < 0)
                    lut[i] = 0;
            }

            var bitmap = new WriteableBitmap((BitmapSource)ModifiedImgSingleton.Source);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;

            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get values of pixels
                var r = pixels[j + 2];
                var g = pixels[j + 1];
                var b = pixels[j];

                //assign new values from lut at r/g/b positions
                pixels[j + 2] = (byte)lut[r];
                pixels[j + 1] = (byte)lut[g];
                pixels[j] = (byte)lut[b];

                j += 4;
            }

            //overwrite pixels table
            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            //set new image
            ModifiedImgSingleton.Source = bitmap;
            //calculate histograms with new image
            HistogramTools.CalculateHistograms();
        }

        #endregion

        #region Binarisation

        private void Binarisation_OwnTreshold(object sender, RoutedEventArgs e)
        {
            if (Binarization.IsImageColoured((BitmapSource)ModifiedImgSingleton.Source))
            {
                MessageBox.Show("Obraz musi być czarno-biały", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var tresholdWindow = new BinarisationOwnTreshold();
            tresholdWindow.Show();
        }

        private void Binarisation_AutomaticThesholdOtsu(object sender, RoutedEventArgs e)
        {
            if (Binarization.IsImageColoured((BitmapSource)ModifiedImgSingleton.Source))
            {
                MessageBox.Show("Obraz musi być czarno-biały", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var threshold = Binarization.GetWariancjaMiędzyklasowa((BitmapSource)ModifiedImgSingleton.Source);

            ModifiedImgSingleton.Source =
                Binarization.ManualBinarisation((BitmapSource)ModifiedImgSingleton.Source, threshold);
        }

        private void Binarisation_LocalTresholdNiblack(object sender, RoutedEventArgs e)
        {
            if (Binarization.IsImageColoured((BitmapSource)ModifiedImgSingleton.Source))
            {
                MessageBox.Show("Obraz musi być czarno-biały", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var niblack = new Niblack();
            niblack.Show();
        }

        private void TurnImageBlackWhiteOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Binarization.TurnImageBlackWhite((BitmapSource)ModifiedImgSingleton.Source);
            HistogramTools.CalculateHistograms();
        }

        #endregion

        #endregion

        #region ResolutionOfImage

        private void ResolutionX_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SaveImage.X = int.Parse(ResolutionX.Text);
                SaveImage.Y = int.Parse(ResolutionY.Text);
            }
            catch (Exception)
            {
            }
        }

        private void ResolutionY_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SaveImage.X = int.Parse(ResolutionX.Text);
                SaveImage.Y = int.Parse(ResolutionY.Text);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region GetWantedStateOfImage (Back and forward)

        private void BackToOriginalImage(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = OriginalImage.Source;
            HistogramTools.CalculateHistograms();
        }

        private void ModifiedImageBack(object sender, RoutedEventArgs e)
        {
        }

        private void ModifiedImageForward(object sender, RoutedEventArgs e)
        {
        }

        #endregion

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
            _originalImgBitmap = ConvertToAbgrImage(_originalImgBitmap);

            OriginalImage.Source = _originalImgBitmap;
            ModifiedImage.Source = _originalImgBitmap;
            ModifiedImgSingleton = ModifiedImage;


            var width = _originalImgBitmap.PixelWidth.ToString();
            var height = _originalImgBitmap.PixelHeight.ToString();
            ResolutionStatusBar.Text = width + " x " + height;
            ResolutionX.Text = width;
            ResolutionY.Text = height;
            ResetPositionAndZoomOfImage();
            SaveImage.X = int.Parse(ResolutionX.Text);
            SaveImage.Y = int.Parse(ResolutionY.Text);
            _originalMatrix = (MatrixTransform)OriginalImage.RenderTransform;
            _modifiedMatrix = (MatrixTransform)ModifiedImage.RenderTransform;

            HistogramTools.CalculateHistograms();
        }

        private static BitmapSource ConvertToAbgrImage(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            return source;
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
            var bitmap = ModifiedImgSingleton.Source;
            SaveImageToFile((BitmapSource)bitmap);
        }

        #endregion

        #region Zooming and Panning

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var point = e.GetPosition(Equals(sender, OriginalImage) ? OriginalImage : ModifiedImage);

            var stOriginal = (MatrixTransform)OriginalImage.RenderTransform;
            var stCopy = (MatrixTransform)ModifiedImage.RenderTransform;
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


            try
            {
                if (e.ClickCount != 2) return;
                var image = (Image)sender;

                var proportionheight = _originalImgBitmap.PixelHeight / image.ActualHeight;
                var proportionwidth = _originalImgBitmap.PixelWidth / image.ActualWidth;
                var point = e.GetPosition(OriginalImage);
                var x = point.X * proportionwidth;
                var y = point.Y * proportionheight;
                CoordinatesXy.Text = "X: " + (int)x + " Y: " + (int)y;

                //OriginalImage.Source = SquareTesting.MarkSquares(originalBitmapSource, (int)x, (int)y);
                //ModifiedImage.Source = SquareTesting.CheckForPotentialMinutia(originalBitmapSource, (int)x, (int)y);
                ModifiedImgSingleton.Source = SquareTesting.DeleteRepetations((BitmapSource)ModifiedImgSingleton.Source, (int)x, (int)y);
                pixels = new byte[4];

                var bitmap = new CroppedBitmap(_originalImgBitmap,
                    new Int32Rect((int)x, (int)y, 1, 1));

                try
                {
                    bitmap.CopyPixels(pixels, 4, 0);
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.StackTrace);
                }

                //var changePixel = new RgbDialog(pixels, point, _originalImgBitmap);
                //changePixel.Show();
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

        private void Thining(object sender, RoutedEventArgs e)
        {
            if (!K3M.IsImageBinarizated((BitmapSource)ModifiedImgSingleton.Source))
            {
                MessageBox.Show("Obraz musi zostać zbinaryzowany!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ModifiedImgSingleton.Source = K3M.ThinningWithK3M((BitmapSource)ModifiedImgSingleton.Source);

        }

        private void MinutiaOnClick(object sender, RoutedEventArgs e)
        {
            ModifiedImgSingleton.Source = Minutia.MarkMinuties((BitmapSource)ModifiedImgSingleton.Source);
            ModifiedImgSingleton.Source = Minutia.DeleteRepetatiobns((BitmapSource)ModifiedImgSingleton.Source);
            ModifiedImgSingleton.Source = Minutia.RemoveRedundantMinutiaes((BitmapSource)ModifiedImgSingleton.Source);
            ModifiedImgSingleton.Source = Minutia.DeleteFalseMinutiaes((BitmapSource)ModifiedImgSingleton.Source, 20);
            ModifiedImgSingleton.Source = Minutia.MarkEndings((BitmapSource)ModifiedImgSingleton.Source);
        }
    }
}