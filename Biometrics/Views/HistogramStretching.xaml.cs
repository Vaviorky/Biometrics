using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Biometrics.Classes;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for HistogramStretching.xaml
    /// </summary>
    public partial class HistogramStretching : Window
    {
        private static int[] HistogramR;
        private static int[] HistogramG;
        private static int[] HistogramB;
        private static int[] HistogramU;

        private byte _option;

        public HistogramStretching()
        {
            InitializeComponent();
            InitHandle();
            InitPreviewImage();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combobox = (ComboBox) sender;

            //handle selection of item from combobox
            switch (combobox.SelectedItem.ToString().Split(new[] {": "}, StringSplitOptions.None).Last())
            {
                case "Czerwony":
                    HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramTools.HistogramR);
                    HistogramRed.Fill = Brushes.Red;
                    _option = 0;
                    break;
                case "Zielony":
                    HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramTools.HistogramG);
                    HistogramRed.Fill = Brushes.Green;
                    _option = 1;

                    break;
                case "Niebieski":
                    HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramTools.HistogramB);
                    HistogramRed.Fill = Brushes.Blue;
                    _option = 2;
                    break;
                case "Uśredniony":
                    HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramTools.HistogramU);
                    HistogramRed.Fill = Brushes.Gray;
                    _option = 3;
                    break;
            }

            PreviewImageUpdate();
        }

        private void InitHandle()
        {
            HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramTools.HistogramR);
        }

        private void InitPreviewImage()
        {
            try
            {
                //set preview image
                PreviewImage.Source = new WriteableBitmap((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }


        private void TextBoxMinValueOnChanged(object sender, TextChangedEventArgs e)
        {
            //handle no value in textbox
            if (TextBoxMin.Text == "")
                return;
            try
            {
                //handle value higer than 255
                if (int.Parse(TextBoxMin.Text) > 255)
                    return;
                //if value is within range, call a function to calculate stretching and update image
                if (int.Parse(TextBoxMin.Text) <= 255 && int.Parse(TextBoxMin.Text) < int.Parse(TextBoxMax.Text))
                    PreviewImageUpdate();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace);
            }
        }


        private void TextBoxMaxValueChanged(object sender, TextChangedEventArgs e)
        {
            //handle no value in textbox
            if (TextBoxMax.Text == "")
                return;
            //handle value higher than 255
            if (int.Parse(TextBoxMax.Text) > 255)
                return;
            //if value is within range, call a function to calculate stretching and update image
            if (int.Parse(TextBoxMax.Text) <= 255 && int.Parse(TextBoxMin.Text) < int.Parse(TextBoxMax.Text))
                PreviewImageUpdate();
        }

        private void PreviewImageUpdate()
        {
            try
            {
                //get lut for stretching - preview image
                var lut = HistogramTools.GetLutStretching(int.Parse(TextBoxMin.Text), int.Parse(TextBoxMax.Text));
                //set new image after histogram stretching
                PreviewImage.Source = HistogramTools.StretchHistogram(lut, _option);
                //recalculate histogram after stretching
                CalculateHistograms();
                //update polygon graphs for visual representation of histograms
                switch (_option)
                {
                    case 0:
                        HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramR);
                        break;
                    case 1:
                        HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramG);
                        break;
                    case 2:
                        HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramB);
                        break;
                    case 3:
                        HistogramRed.Points = HistogramWindow.ConvertToPointCollection(HistogramU);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            //close the window
            Close();
            //perform lut calculation for main image
            var lut = HistogramTools.GetLutStretching(int.Parse(TextBoxMin.Text), int.Parse(TextBoxMax.Text));
            //update image with newer one after histogram stretching
            MainWindow.ModifiedImgSingleton.Source = HistogramTools.StretchHistogram(lut, _option);
            //recalculate histograms for updated image
            HistogramTools.CalculateHistograms();
        }

        #region PreviewImageHistogramCalculation

        private void CalculateHistograms()
        {
            //interer arrays to store how many times each of each intensity value in image occurs
            HistogramR = new int[256];
            HistogramG = new int[256];
            HistogramB = new int[256];
            HistogramU = new int[256];

            var imgSource = (BitmapSource) PreviewImage.Source;

            //bitmap from which we can get all pixels values
            var bitmap = new WriteableBitmap(imgSource);

            var width = imgSource.PixelWidth;
            var height = imgSource.PixelHeight;

            var stride = width * ((imgSource.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get pixels channels values
                var r = pixels[j + 2];
                var g = pixels[j + 1];
                var b = pixels[j];

                //value++ of chanel in position which is equal to r/g/b value
                HistogramR[r]++;
                HistogramG[g]++;
                HistogramB[b]++;

                //calculate average value of position i in histogram
                var temp = (r + g + b) / 3;
                HistogramU[temp]++;

                j += 4;
            }
        }

        #endregion
    }
}