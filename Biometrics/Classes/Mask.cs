using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace Biometrics.Classes
{
    public static class Mask
    {
        public static BitmapSource PerformmmMask(int[,] mask, int size)
        {
            var imgSource = (BitmapSource) MainWindow.ModifiedImgSingleton.Source;

            var offset = (size - 1) / 2;
            var max = 0;
            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                max += mask[i, j];
            max = max == 0 ? 1 : max;

            var bitmap = new WriteableBitmap(imgSource);
            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stride = width * ((imgSource.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            var pixels = new byte[arraySize];

            bitmap.CopyPixels(pixels, stride, 0);
            int pixelR = 0, pixelG = 0, pixelB = 0;
            int xx = 0, y = 0;
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                if (y > height)
                    y = 0;
                var t = y++ - offset;
                Debug.WriteLine(t);
                for (var j = 0; j < size; j++)
                {
                    if (t < 0)
                    {
                    }
                    var nx = xx - offset * 4;
                    for (var k = 0; k < size; k++)
                    {
                        if (nx < 0)
                            nx = 0;
                        else
                            nx = nx > width - 4 ? width - 4 : nx;
                        pixelB += pixels[nx] * mask[i, j];
                        pixelG += pixels[nx + 1] * mask[i, j];
                        pixelR += pixels[nx + 2] * mask[i, j];
                        nx += 4;
                    }


                    t++;
                }
                pixelB = pixelB / max;
                pixelG = pixelG / max;
                pixelR = pixelR / max;

                if (pixelB > 255 || pixelB < 0)
                    pixelB = pixelB > 255 ? 255 : 0;
                if (pixelG > 255 || pixelG < 0)
                    pixelG = pixelG > 255 ? 255 : 0;
                if (pixelR > 255 || pixelR < 0)
                    pixelR = pixelR > 255 ? 255 : 0;

                pixels[xx] = (byte) pixelB;
                pixels[xx + 1] = (byte) pixelG;
                pixels[xx + 2] = (byte) pixelR;

                xx += 4;
            }
            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);

            return bitmap;
        }

        public static BitmapSource PerformMask(int[,] mask, int size)
        {
            var image = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            var sourceImage = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);

            var offset = (size - 1) / 2;
            var max = 0;

            for (var i = 0; i < size; i++)
            for (var j = 0; j < size; j++)
                max += mask[i, j];

            max = max == 0 ? 1 : max;

            unsafe
            {
                //original image
                var bitmapData = sourceImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                    ImageLockMode.ReadOnly, sourceImage.PixelFormat);
                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*) bitmapData.Scan0;

                //copy for calculation purposes
                var cData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.WriteOnly, image.PixelFormat);
                var firstPixel = (byte*) cData.Scan0;

                var pixelR = 0;
                var pixelG = 0;
                var pixelB = 0;

                for (var y = 0; y < heightInPixels; y++)
                {
                    var currentLine1 = firstPixel + y * cData.Stride;
                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        var t = y - offset;

                        for (var i = 0; i < size; i++)
                        {
                            byte* currentLine;
                            if (t < 0)
                                currentLine = ptrFirstPixel + 0 * bitmapData.Stride;
                            else
                                currentLine = t > heightInPixels - 1
                                    ? ptrFirstPixel + (heightInPixels - 1) * bitmapData.Stride
                                    : ptrFirstPixel + t * bitmapData.Stride;

                            var newPixel = x - offset * bytesPerPixel;

                            for (var j = 0; j < size; j++)
                            {
                                if (newPixel < 0)
                                    newPixel = x;
                                else
                                    newPixel = newPixel > widthInBytes - bytesPerPixel ? widthInBytes - bytesPerPixel : newPixel;

                                pixelB += currentLine[newPixel] * mask[i, j];
                                pixelG += currentLine[newPixel + 1] * mask[i, j];
                                pixelR += currentLine[newPixel + 2] * mask[i, j];

                                newPixel += bytesPerPixel;
                            }
                            t++;
                        }

                        pixelB = pixelB / max;
                        pixelG = pixelG / max;
                        pixelR = pixelR / max;

                        if (pixelB > 255 || pixelB < 0)
                            pixelB = pixelB > 255 ? 255 : 0;

                        if (pixelG > 255 || pixelG < 0)
                            pixelG = pixelG > 255 ? 255 : 0;

                        if (pixelR > 255 || pixelR < 0)
                            pixelR = pixelR > 255 ? 255 : 0;

                        currentLine1[x] = (byte) pixelB;
                        currentLine1[x + 1] = (byte) pixelG;
                        currentLine1[x + 2] = (byte) pixelR;

                        pixelB = 0;
                        pixelG = 0;
                        pixelR = 0;
                    }
                }
                sourceImage.UnlockBits(bitmapData);
                image.UnlockBits(cData);

                return ConvertBitmap(image);
            }
        }

        public static async Task<BitmapSource> PerformMask_Kuwahar(int size, IProgress<int> progress)
        {
            var image = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            var sourceBitmap = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);

            var offset = (size - 1) / 2;
            var maximum = sourceBitmap.Width * sourceBitmap.Height;
            await Task.Run(() =>
            {
                unsafe
                {
                    //original image
                    var bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                        ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
                    var bytesPerPixel = Image.GetPixelFormatSize(sourceBitmap.PixelFormat) / 8;
                    var heightInPixels = bitmapData.Height;
                    var widthInBytes = bitmapData.Width * bytesPerPixel;
                    var ptrFirstPixel = (byte*) bitmapData.Scan0;

                    //copy for calculation purposes
                    var cData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.WriteOnly, image.PixelFormat);
                    var firstPixel = (byte*) cData.Scan0;
                    var i = 0;
                    for (var y = 0; y < heightInPixels; y++)
                    {
                        var currentLine1 = firstPixel + y * cData.Stride;
                        for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            byte* currentLine;

                            var x1 = x - offset * bytesPerPixel;
                            var y1 = y - offset;

                            var x2 = x;
                            var y2 = y - offset;

                            var x3 = x;
                            var y3 = y;

                            var x4 = x - offset * bytesPerPixel;
                            var y4 = y;

                            double maxR = int.MaxValue;
                            double averageR = 0;
                            double maxG = int.MaxValue;
                            double averageG = 0;
                            double maxB = int.MaxValue;
                            double averageB = 0;

                            var pixelsMax = Math.Pow(offset + 1, 2);
                            var rValue = new double[offset + 1, offset + 1];
                            var gValue = new double[offset + 1, offset + 1];
                            var bValue = new double[offset + 1, offset + 1];

                            int t;
                            if (x1 >= 0 && y1 >= 0)
                            {
                                t = y - offset;
                                for (var n = 0; n <= offset; n++)
                                {
                                    currentLine = ptrFirstPixel + t * bitmapData.Stride;
                                    for (var m = 0; m <= offset; m++)
                                    {
                                        var poz = x1 + m * bytesPerPixel;
                                        bValue[n, m] = currentLine[poz];
                                        gValue[n, m] = currentLine[poz + 1];
                                        rValue[n, m] = currentLine[poz + 2];
                                    }
                                    t++;
                                }

                                //for r g and b calculate average and variation
                                var r = KuwaharaCalcculation(rValue, pixelsMax, maxR, averageR);
                                maxR = r[0];
                                averageR = r[1];

                                var g = KuwaharaCalcculation(gValue, pixelsMax, maxG, averageG);
                                maxG = g[0];
                                averageG = g[1];

                                var b = KuwaharaCalcculation(bValue, pixelsMax, maxB, averageB);
                                maxB = b[0];
                                averageB = b[1];
                            }
                            if (x2 + offset * bytesPerPixel <= widthInBytes - bytesPerPixel && y2 >= 0)
                            {
                                t = y - offset;
                                for (var n = 0; n <= offset; n++)
                                {
                                    currentLine = ptrFirstPixel + t * bitmapData.Stride;
                                    for (var m = 0; m <= offset; m++)
                                    {
                                        var poz = x2 + m * bytesPerPixel;
                                        bValue[n, m] = currentLine[poz];
                                        gValue[n, m] = currentLine[poz + 1];
                                        rValue[n, m] = currentLine[poz + 2];
                                    }
                                    t++;
                                }

                                //for r g and b calculate average and variation
                                var r = KuwaharaCalcculation(rValue, pixelsMax, maxR, averageR);
                                maxR = r[0];
                                averageR = r[1];
                                var g = KuwaharaCalcculation(gValue, pixelsMax, maxG, averageG);
                                maxG = g[0];
                                averageG = g[1];
                                var b = KuwaharaCalcculation(bValue, pixelsMax, maxB, averageB);
                                maxB = b[0];
                                averageB = b[1];
                            }

                            if (x3 + offset * bytesPerPixel <= widthInBytes - bytesPerPixel
                                && y3 + offset <= heightInPixels - 1)
                            {
                                t = y;
                                for (var n = 0; n <= offset; n++)
                                {
                                    currentLine = ptrFirstPixel + t * bitmapData.Stride;
                                    for (var m = 0; m <= offset; m++)
                                    {
                                        var poz = x3 + m * bytesPerPixel;
                                        bValue[n, m] = currentLine[poz];
                                        gValue[n, m] = currentLine[poz + 1];
                                        rValue[n, m] = currentLine[poz + 2];
                                    }
                                    t++;
                                }

                                //for r g and b calculate average and variation
                                var r = KuwaharaCalcculation(rValue, pixelsMax, maxR, averageR);
                                maxR = r[0];
                                averageR = r[1];
                                var g = KuwaharaCalcculation(gValue, pixelsMax, maxG, averageG);
                                maxG = g[0];
                                averageG = g[1];
                                var b = KuwaharaCalcculation(bValue, pixelsMax, maxB, averageB);
                                maxB = b[0];
                                averageB = b[1];
                            }
                            if (x4 >= 0 && y4 + offset <= heightInPixels - 1)
                            {
                                t = y;
                                for (var n = 0; n <= offset; n++)
                                {
                                    currentLine = ptrFirstPixel + t * bitmapData.Stride;
                                    for (var m = 0; m <= offset; m++)
                                    {
                                        var poz = x4 + m * bytesPerPixel;
                                        bValue[n, m] = currentLine[poz];
                                        gValue[n, m] = currentLine[poz + 1];
                                        rValue[n, m] = currentLine[poz + 2];
                                    }
                                    t++;
                                }

                                //for r g and b calculate average and variation
                                var r = KuwaharaCalcculation(rValue, pixelsMax, maxR, averageR);
                                maxR = r[0];
                                averageR = r[1];
                                var g = KuwaharaCalcculation(gValue, pixelsMax, maxG, averageG);
                                maxG = g[0];
                                averageG = g[1];
                                var b = KuwaharaCalcculation(bValue, pixelsMax, maxB, averageB);
                                maxB = b[0];
                                averageB = b[1];
                            }


                            //result - get average
                            averageR = Math.Round(averageR, 0, MidpointRounding.AwayFromZero);
                            averageG = Math.Round(averageG, 0, MidpointRounding.AwayFromZero);
                            averageB = Math.Round(averageB, 0, MidpointRounding.AwayFromZero);

                            if (averageR > 255 || averageR < 0)
                                averageR = averageR > 255 ? 255 : 0;
                            if (averageG > 255 || averageG < 0)
                                averageG = averageG > 255 ? 255 : 0;
                            if (averageB > 255 || averageB < 0)
                                averageB = averageB > 255 ? 255 : 0;

                            // calculate new pixel value
                            currentLine1[x] = (byte) averageB;
                            currentLine1[x + 1] = (byte) averageG;
                            currentLine1[x + 2] = (byte) averageR;

                            //progress
                            if (progress != null)
                            {
                                var value = i * 100 / maximum;
                                progress.Report(value);
                            }
                            i++;
                        }
                    }
                    sourceBitmap.UnlockBits(bitmapData);
                    image.UnlockBits(cData);
                    progress?.Report(100);
                }
            });
            MainWindow.ModifiedImgSingleton.Source = ConvertBitmap(image);
            return ConvertBitmap(image);
        }

        public static async Task<BitmapSource> PerformMask_Median(int size, IProgress<int> progress)
        {
            var image = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);
            var sourceBitmap = BitmapFromSource((BitmapSource) MainWindow.ModifiedImgSingleton.Source);

            var offset = (size - 1) / 2;
            var max = sourceBitmap.Width * sourceBitmap.Height;

            await Task.Run(() =>
            {
                unsafe
                {
                    //original image
                    var bitmapData = sourceBitmap.LockBits(new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                        ImageLockMode.ReadOnly, sourceBitmap.PixelFormat);
                    var bytesPerPixel = Image.GetPixelFormatSize(sourceBitmap.PixelFormat) / 8;
                    var heightInPixels = bitmapData.Height;
                    var widthInBytes = bitmapData.Width * bytesPerPixel;
                    var ptrFirstPixel = (byte*) bitmapData.Scan0;

                    //copy for calculation purposes
                    var cData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.WriteOnly, image.PixelFormat);
                    var firstPixel = (byte*) cData.Scan0;

                    var pixelR = new List<int>();
                    var pixelG = new List<int>();
                    var pixelB = new List<int>();

                    var v = 0;

                    for (var y = 0; y < heightInPixels; y++)
                    {
                        var currentLine1 = firstPixel + y * cData.Stride;
                        for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                        {
                            var t = y - offset;
                            for (var i = 0; i < size; i++)
                            {
                                byte* currentLine;
                                if (t < 0)
                                    currentLine = ptrFirstPixel + 0 * bitmapData.Stride;
                                else
                                    currentLine = t > heightInPixels - 1
                                        ? ptrFirstPixel + (heightInPixels - 1) * bitmapData.Stride
                                        : ptrFirstPixel + t * bitmapData.Stride;

                                var nx = x - offset * bytesPerPixel;

                                for (var j = 0; j < size; j++)
                                {
                                    if (nx < 0)
                                        nx = x;
                                    else
                                        nx = nx > widthInBytes - bytesPerPixel ? widthInBytes - bytesPerPixel : nx;

                                    pixelB.Add(currentLine[nx]);
                                    pixelG.Add(currentLine[nx + 1]);
                                    pixelR.Add(currentLine[nx + 2]);
                                    nx += bytesPerPixel;
                                }
                                t++;
                            }

                            //search for median from all pixels under mask and set pixel value from it
                            currentLine1[x] = (byte) SearchMedian(pixelB);
                            currentLine1[x + 1] = (byte) SearchMedian(pixelG);
                            currentLine1[x + 2] = (byte) SearchMedian(pixelR);

                            v++;

                            pixelB.Clear();
                            pixelG.Clear();
                            pixelR.Clear();
                            if (progress != null)
                                progress.Report(v * 100 / max);
                        }
                    }
                    sourceBitmap.UnlockBits(bitmapData);
                    image.UnlockBits(cData);
                    if (progress != null)
                        progress.Report(100);
                }
            });
            return ConvertBitmap(image);
        }

        //get array of average value and variation from it
        private static double[] KuwaharaCalcculation(double[,] colorValue, double maxPixes, double max, double average)
        {
            var averageTemp = GetAverage(colorValue, maxPixes);
            var variationTemp = GetVariation(colorValue, maxPixes, averageTemp);

            var result = new double[2];

            if (max > variationTemp)
            {
                result[0] = variationTemp;
                result[1] = averageTemp;
            }
            else
            {
                result[0] = max;
                result[1] = average;
            }
            return result;
        }

        private static double GetAverage(double[,] array, double max)
        {
            var suma = array.Cast<double>().Sum();
            return suma / max;
        }

        private static double GetVariation(double[,] array, double max, double average)
        {
            var suma = array.Cast<double>().Sum(arg => Math.Pow(arg - average, 2));
            return suma / max;
        }

        private static int SearchMedian(List<int> array)
        {
            array.Sort();
            var medium = (array.Count - 1) / 2;
            medium += 1;
            return array[medium];
        }

        private static BitmapSource ConvertBitmap(Bitmap source)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        private static Bitmap BitmapFromSource(BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }
    }
}