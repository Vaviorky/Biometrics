using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class HistogramTools
    {
        public static int[] HistogramR;
        public static int[] HistogramG;
        public static int[] HistogramB;
        public static int[] HistogramU;
        public static int MaxPixels;

        public static void CalculateHistograms()
        {
            //interer arrays to store how many times each of each intensity value in image occurs
            HistogramR = new int[256];
            HistogramG = new int[256];
            HistogramB = new int[256];
            HistogramU = new int[256];

            var imgSource = (BitmapSource) MainWindow.ModifiedImgSingleton.Source;

            MaxPixels = imgSource.PixelWidth * imgSource.PixelHeight;

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

        public static double[] GetDystrybuanta(int[] histogram)
        {
            double suma = 0;
            var dystrybuanta = new double[histogram.Length];
            for (var i = 0; i < histogram.Length; i++)
            {
                suma += histogram[i];
                dystrybuanta[i] = suma / MaxPixels;
            }
            return dystrybuanta;
        }

        public static int[] GetLutEqualization(double[] dystrybuanta, int max)
        {
            var i = 0;
            //loop through dystrybuanta table while it's values are equal to zero
            while (dystrybuanta[i] == 0)
                i++;

            //assign this value to new variable
            var nonzero = dystrybuanta[i];

            var lut = new int[256];

            for (i = 0; i < 256; i++)
            {
                //calculate value of lut in position i with given formula
                lut[i] = (int) ((dystrybuanta[i] - nonzero) / (1 - nonzero) * (max - 1));

                //check if calculated value fits in range [0..255]
                if (lut[i] > 255)
                    lut[i] = 255;

                if (lut[i] < 0)
                    lut[i] = 0;
            }
            return lut;
        }

        public static WriteableBitmap EqualizeHistogram(int[] lutR, int[] lutG, int[] lutB)
        {
            var imgSource = (BitmapSource) MainWindow.ModifiedImgSingleton.Source;
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

                //set new values according to lut
                pixels[j + 2] = (byte) lutR[r];
                pixels[j + 1] = (byte) lutG[g];
                pixels[j] = (byte) lutB[b];

                j += 4;
            }

            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        public static int[] GetLutStretching(double min, double max)
        {
            var lut = new int[256];

            for (var i = 0; i < 256; i++)
            {
                //formula for calculating lut[i] value
                var value = (255.0 / (max - min)) * (i - min);

                //checking if calculated value fits in range [0,255]
                if (value > 255)
                    lut[i] = 255;

                else if (value < 0)
                    lut[i] = 0;

                else
                    lut[i] = (int) Math.Round(value, 0, MidpointRounding.AwayFromZero);
            }
            return lut;
        }

        public static WriteableBitmap StretchHistogram(int[] lut, byte layer)
        {
            var bitmap = new WriteableBitmap((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);
            var arraySize = stride * height;

            var pixels = new byte[arraySize];

            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;

            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get values of color channels
                var r = pixels[j + 2];
                var g = pixels[j + 1];
                var b = pixels[j];
                //set channel with given information about lut according to user choice
                switch (layer)
                {
                    case 0:
                        pixels[j + 2] = (byte) lut[r];
                        break;
                    case 1:
                        pixels[j + 1] = (byte)lut[g];
                        break;
                    case 2:
                        pixels[j] = (byte)lut[b];
                        break;
                    case 3:
                        pixels[j + 2] = (byte)lut[r];
                        pixels[j + 1] = (byte)lut[g];
                        pixels[j] = (byte)lut[b];
                        break;
                }

                j += 4;
            }

            //overwrite pixels values with calculated ones
            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        
    }
}