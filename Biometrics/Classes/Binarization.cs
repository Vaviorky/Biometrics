using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Biometrics.Classes
{
    public class Binarization
    {
        public static bool IsImageColoured(BitmapSource image)
        {
            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

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

                //if values are different from each other, then image is coloured
                if (r != g || r != b || g != b)
                    return true;

                j += 4;
            }
            return false;
        }

        public static WriteableBitmap TurnImageBlackWhite(BitmapSource image)
        {
            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

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

                //set pixels to grayscale
                var s = (int) Math.Round((b + g + r) / 3.0, 0, MidpointRounding.AwayFromZero);

                pixels[j + 2] = (byte) s;
                pixels[j + 1] = (byte) s;
                pixels[j] = (byte) s;

                j += 4;
            }

            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        public static WriteableBitmap ManualBinarisation(BitmapSource image, int treshhold)
        {
            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get pixel gray value
                var oldPixelValue = pixels[j];

                //set pixels to grayscale
                var value = oldPixelValue <= treshhold ? 0 : 255;

                pixels[j + 2] = (byte) value;
                pixels[j + 1] = (byte) value;
                pixels[j] = (byte) value;

                j += 4;
            }

            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }


        private static int[] GetPixelsValuesArray(BitmapSource image)
        {
            var tab = new int[256];

            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get pixel value
                var value = pixels[j + 2];

                tab[value]++;
                j += 4;
            }

            return tab;
        }

        private static double GetWeight(int[] pixelsValues, int maxPixels, int start, int end)
        {
            double suma = 0;
            for (var i = start; i < end; i++)
                suma += pixelsValues[i];
            return suma / maxPixels;
        }

        private static double GetAverage(int[] pixelsValues, int start, int end)
        {
            double max = 0;
            double suma = 0;
            for (var i = start; i < end; i++)
            {
                suma += pixelsValues[i] * i;
                max += pixelsValues[i];
            }
            return suma / max;
        }

        private static double GetVariation(int[] histogram, double srednia)
        {
            double temp = 0;
            double max = 0;
            for (var k = 0; k < histogram.Length; k++)
            {
                temp += histogram[k] * Math.Pow(k - srednia, 2.0);
                max += histogram[k];
            }
            return Math.Sqrt(temp / max);
        }

        public static int GetWariancjaMiędzyklasowa(BitmapSource image)
        {
            var pixelsTab = GetPixelsValuesArray(image);

            var bgWeight = new double[256];
            var bgMean = new double[256];


            var fgWeight = new double[256];
            var fgMean = new double[256];

            var max = image.PixelWidth * image.PixelHeight;
            for (var i = 0; i < 256; i++)
            {
                bgWeight[i] = GetWeight(pixelsTab, max, 0, i);
                bgMean[i] = GetAverage(pixelsTab, 0, i);

                fgWeight[i] = GetWeight(pixelsTab, max, i + 1, 256);
                fgMean[i] = GetAverage(pixelsTab, i + 1, 256);
            }

            double maximum = 0;
            var pozycja = 0;
            for (var j = 0; j < 256; j++)
            {
                var wynik = bgWeight[j] * fgWeight[j] * (bgMean[j] - fgMean[j]) * (bgMean[j] - fgMean[j]);
                if (wynik > maximum)
                {
                    maximum = wynik;
                    pozycja = j;
                }
            }
            return pozycja;
        }

        public async Task<WriteableBitmap> PerformNiblack(double k, int windowSize, IProgress<int> progress)
        {
            var img = new WriteableBitmap((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            var image1 = GetBitmap(img);
            var image2 = GetBitmap(img);

            var tresholdList1 = new List<int>();
            var tresholdList2 = new List<int>();

            var r = (windowSize - 1) / 2;
            var width = image1.Width;
            var imgHeight = image1.Height;
            var z = 1;

            var imgSize = new Size(width, imgHeight);

            var pierwszaWysokosc = imgHeight / 2;

            //pararell calculation of thresholds
            await Task.Run(() =>
            {
                Parallel.Invoke(() =>
                {
                    for (var y = 0; y < pierwszaWysokosc; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var prog = GetThreshold(imgSize, image1, r, k, windowSize, x, y);
                        tresholdList1.Add(prog);
                        if (progress != null)
                            progress.Report(z);
                        z++;
                    }
                }, () =>
                {
                    for (var y = pierwszaWysokosc; y <= imgHeight; y++)
                    for (var x = 0; x < width; x++)
                    {
                        var prog = GetThreshold(imgSize, image2, r, k, windowSize, x, y);
                        tresholdList2.Add(prog);
                    }
                });
            });

            image1.Dispose();
            image2.Dispose();

            //merge two lists
            tresholdList1.AddRange(tresholdList2);

            var bitmap = new WriteableBitmap(img);

            var widthNew = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = widthNew * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get pixel gray value
                var oldPixelValue = pixels[j];
                int value;
                //set pixels to grayscale
                if (oldPixelValue <= tresholdList1[i])
                    value = 0;
                else
                    value = 255;

                pixels[j + 2] = (byte) value;
                pixels[j + 1] = (byte) value;
                pixels[j] = (byte) value;

                j += 4;
            }

            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        private static int GetThreshold(Size dimensions, Bitmap image, int r, double k, int windowSize, int x, int y)
        {
            //calculate subimage coordinates
            var x1 = x - r;
            var x2 = x + r;
            var y1 = y - r;
            var y2 = y + r;

            //check for bounds
            if (x1 < 0)
                x1 = 0;

            if (x2 > dimensions.Width)
                x2 = dimensions.Width;

            if (y1 < 0)
                y1 = 0;

            if (y2 > dimensions.Height)
                y2 = dimensions.Height;

            //create subimage
            var window = new Bitmap(x2 - x1, y2 - y1);
            using (var g = Graphics.FromImage(window))
            {
                var rect = new Rectangle(x1, y1, x2 - x1, y2 - y1);
                g.DrawImage(image, 0, 0, rect, GraphicsUnit.Pixel);
            }

            //get histogram of subimage
            var histogram = GetHistogramUsredniony(window);

            //check for difference
            if (window.Width * window.Height < windowSize * windowSize)
            {
                var diff = windowSize * windowSize - window.Width * window.Height;
                histogram[0] += diff;
            }

            //get average and variation
            var average = GetAverage(histogram, 0, histogram.Length);
            var variation = GetVariation(histogram, average);

            //calculate threshold
            var threshold = (int) Math.Round(average + k * variation, 0, MidpointRounding.AwayFromZero);
            window.Dispose();
            return threshold;
        }

        public static int[] GetHistogramUsredniony(Bitmap obrazek)
        {
            var bmp = new Bitmap(obrazek);
            var histogram = new int[256];

            unsafe
            {
                var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite,
                    bmp.PixelFormat);
                var bytesPerPixel = Image.GetPixelFormatSize(bmp.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*) bitmapData.Scan0;

                for (var y = 0; y < heightInPixels; y++)
                {
                    var currentLine = ptrFirstPixel + y * bitmapData.Stride;
                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        //usredniony
                        var tmp = 0;
                        tmp += oldRed;
                        tmp += oldGreen;
                        tmp += oldBlue;
                        tmp = tmp / 3;
                        histogram[tmp]++;
                    }
                }
                bmp.UnlockBits(bitmapData);
            }
            return histogram;
        }

        private static Bitmap GetBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}