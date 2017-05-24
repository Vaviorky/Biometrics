using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class K3M
    {
        private static int _width, _height, _stride;
        private static byte[] _pixels;
        private static int[,] intPixels;

        private static List<int[]> blackPixels;
        private static List<int[]> adjacentPixels;

        private static bool somethingChanged;


        private static readonly int[][] A =
        {
            new[]
            {
                3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131, 135,
                143, 159, 191, 192, 193, 195, 199, 207, 223, 224, 225, 227, 231, 239, 240, 241, 243, 247, 248, 249, 251,
                252, 253, 254
            },
            new[] {7, 14, 28, 56, 112, 131, 193, 224},
            new[] {7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135, 193, 195, 224, 225, 240},
            new[]
            {
                7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120, 124, 131, 135, 143, 193, 195, 199, 224, 225, 227, 240, 241,
                248
            },
            new[]
            {
                7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120, 124, 126, 131, 135, 143, 159, 193, 195, 199, 207, 224,
                225, 227, 231, 240, 241, 243, 248, 249, 252
            },
            new[]
            {
                7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120, 124, 126, 131, 135, 143, 159, 191, 193, 195, 199, 207,
                224, 225, 227, 231, 239, 240, 241, 243, 248, 249, 251, 252, 254
            }
        };

        private static readonly int[] A1Pix =
        {
            3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131, 135, 143,
            159, 191, 192, 193, 195, 199, 207, 223, 224, 225, 227, 231, 239, 240, 241, 243, 247, 248, 249, 251, 252,
            253, 254
        };

        private static readonly int[,] Mask =
        {
            {128, 64, 32},
            {1, 0, 16},
            {2, 4, 8}
        };

        const byte BLACK = 1;
        const byte WHITE = 0;

        public static BitmapSource ThinningWithK3M(BitmapSource source)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            blackPixels = new List<int[]>();
            adjacentPixels = new List<int[]>();

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            //get black pixels list
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (intPixels[x, y] == BLACK)
                    {
                        blackPixels.Add(new[] { x, y });
                    }
                }
            }

            somethingChanged = true;
            while (somethingChanged)
            {
                somethingChanged = false;

                MarkPixelsAsAdjacent();
                
                //delete adjacent pixels which sum (surrounding * mask) A1..A5
                for (int i = 1; i <= 5; i++)
                {
                    LoopThroughAMask(i);
                }

                blackPixels.AddRange(adjacentPixels);
                adjacentPixels.Clear();

            }

            A1pixMask();
            
            RevertIntPixelsIntoPixelArray();
            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;

        }

        private static void InitIntImage()
        {
            intPixels = new int[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int j = PositionInArray(x, y);

                    if (_pixels[j + 2] == 255 && _pixels[j + 1] == 255 && _pixels[j] == 255)
                    {
                        intPixels[x, y] = 0;
                    }
                    else if (_pixels[j + 2] == 0 && _pixels[j + 1] == 0 && _pixels[j] == 0)
                    {
                        intPixels[x, y] = 1;
                    }
                }
            }
        }

        private static void RevertIntPixelsIntoPixelArray()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int j = PositionInArray(x, y);

                    if (intPixels[x, y] == 0)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (intPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (intPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                }
            }
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }

        private static void LoopThroughAMask(int mainLoopIndex)
        {
            for (int index = 0; index < adjacentPixels.Count; index++)
            {
                int[] currentPixel = adjacentPixels[index];
                int x = currentPixel[0];
                int y = currentPixel[1];
                int sum = 0;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // if it goes out of image, or is the pixel we are inspecting
                        if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                            continue;

                        if (intPixels[x + j, y + i] != WHITE)
                        {
                            sum += Mask[j + 1, i + 1];
                        }
                    }
                }

                if (A[mainLoopIndex].Contains(sum))
                {
                    somethingChanged = true;
                    adjacentPixels.RemoveAt(index);
                    --index;
                    intPixels[x, y] = 0;        //mark pixel as white
                }
            }
        }

        private static void MarkPixelsAsAdjacent()
        {
            for (int index = 0; index < blackPixels.Count; index++)
            {
                int[] currentPixel = blackPixels[index];
                int x = currentPixel[0];
                int y = currentPixel[1];
                int sum = 0;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // if it goes out of image, or is the pixel we are inspecting
                        if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                            continue;

                        if (intPixels[x + j, y + i] != WHITE)
                        {
                            sum += Mask[j + 1, i + 1];
                        }
                    }
                }

                if (A[0].Contains(sum))
                {
                    blackPixels.RemoveAt(index);
                    --index;
                    adjacentPixels.Add(currentPixel);
                }
            }
        }

        private static void A1pixMask()
        {
            for (int index = 0; index < adjacentPixels.Count; index++)
            {
                int[] currentPixel = adjacentPixels[index];
                int x = currentPixel[0];
                int y = currentPixel[1];
                int sum = 0;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // if it goes out of image, or is the pixel we are inspecting
                        if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                            continue;

                        if (intPixels[x + j, y + i] != WHITE)
                        {
                            sum += Mask[j + 1, i + 1];
                        }
                    }
                }

                if (A1Pix.Contains(sum))
                {
                    blackPixels.RemoveAt(index);
                    --index;
                    adjacentPixels.Add(currentPixel);
                }
            }
        }

        public static bool IsImageBinarizated(BitmapSource image)
        {
            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            var stide = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stide * height;
            var pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stide, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < pixels.Length / 4; i++)
            {
                //get pixels channels values
                var r = pixels[j + 2];
                var g = pixels[j + 1];
                var b = pixels[j];

                //check if image is binary
                if (r != 0 || g != 0 || b != 0)
                {
                    if (r != 255 || g != 255 || b != 255)
                    {
                        return false;
                    }
                }

                j += 4;
            }
            return true;
        }
    }
}
