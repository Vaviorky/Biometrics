using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class SquareTesting
    {
        private static int _width, _height, _stride;
        private static byte[] _pixels, _tempPixels;
        private static int[,] _intPixels, _tempIntPixels;

        public static BitmapSource MarkSquares(BitmapSource source, int x, int y)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            _tempPixels = _pixels;

            InitIntImage();

            _tempIntPixels = _intPixels;

            for (int i = -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (i != -2 && i != 2 && j != -2 && j != 2)
                        continue;

                    if (_intPixels[x + i, y + j] == 1)
                    {
                        continue;
                    }

                    _tempIntPixels[x, y] = 2;

                    _intPixels[x + i, y + j] = 2;
                }
            }

            for (int i = -4; i <= 4; i++)
            {
                for (int j = -4; j <= 4; j++)
                {
                    if (i != -4 && i != 4 && j != -4 && j != 4)
                        continue;

                    if (_intPixels[x + i, y + j] == 1)
                    {
                        continue;
                    }

                    _intPixels[x + i, y + j] = 3;
                }
            }

            RevertIntPixelsIntoPixelArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        public static BitmapSource CheckForPotentialMinutia(BitmapSource source, int x, int y)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            int countForSquare5 = 0, countForSquare9 = 0;
            bool square5 = false, square9 = false;

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            _tempPixels = _pixels;

            InitIntImage();

            _tempIntPixels = _intPixels;
            int[] blacksInSquare5 = GetArrayOfBlacksSquare5(x, y);
            int[] blacksInSquare9 = GetArrayOfBlacksSquare9(x, y);


            foreach (var pixel in blacksInSquare5)
            {
                if (pixel == 1)
                {
                    square5 = true;
                }
                if (pixel == 0 && square5)
                {
                    countForSquare5++;
                    square5 = false;
                }
            }

            foreach (var pixel in blacksInSquare9)
            {
                if (pixel == 1)
                {
                    square9 = true;
                }
                if (pixel == 0 && square9)
                {
                    countForSquare9++;
                    square9 = false;
                }


            }

            MarkEndings(x, y);

         //   Debug.WriteLine("");
          //  Debug.WriteLine("");
         //   Debug.WriteLine("");
         //   Debug.WriteLine(countForSquare5 + " " + countForSquare9);

            if (countForSquare5 == 3 && countForSquare9 == 3)
            {
                _intPixels[x, y] = 2;
            }

            RevertIntPixelsIntoPixelArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        private static void InitIntImage()
        {
            _intPixels = new int[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int j = PositionInArray(x, y);

                    if (_pixels[j + 2] == 255 && _pixels[j + 1] == 255 && _pixels[j] == 255)
                    {
                        _intPixels[x, y] = 0;
                    }
                    else if (_pixels[j + 2] == 0 && _pixels[j + 1] == 0 && _pixels[j] == 0)
                    {
                        _intPixels[x, y] = 1;
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

                    if (_intPixels[x, y] == 0)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (_intPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_intPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_intPixels[x, y] == 3)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 0)
                    {
                        _tempPixels[j + 2] = 255;
                        _tempPixels[j + 1] = 255;
                        _tempPixels[j] = 255;
                    }

                    if (_tempIntPixels[x, y] == 1)
                    {
                        _tempPixels[j + 2] = 0;
                        _tempPixels[j + 1] = 0;
                        _tempPixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 2)
                    {
                        _tempPixels[j + 2] = 255;
                        _tempPixels[j + 1] = 0;
                        _tempPixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 3)
                    {
                        _tempPixels[j + 2] = 0;
                        _tempPixels[j + 1] = 255;
                        _tempPixels[j] = 0;
                    }

                }
            }
        }

        private static int[] GetArrayOfBlacksSquare5(int x, int y)
        {
            int[] blacks = new int[17];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = _intPixels[x - 4, y - 4];
            blacks[1] = _intPixels[x - 1, y - 2];
            blacks[2] = _intPixels[x, y - 2];
            blacks[3] = _intPixels[x + 1, y - 2];
            blacks[4] = _intPixels[x + 2, y - 2];
            blacks[5] = _intPixels[x + 2, y - 1];
            blacks[6] = _intPixels[x + 2, y];
            blacks[7] = _intPixels[x + 2, y + 1];
            blacks[8] = _intPixels[x + 2, y + 2];
            blacks[9] = _intPixels[x + 1, y + 2];
            blacks[10] = _intPixels[x, y + 2];
            blacks[11] = _intPixels[x - 1, y + 2];
            blacks[12] = _intPixels[x - 2, y + 2];
            blacks[13] = _intPixels[x - 2, y + 1];
            blacks[14] = _intPixels[x - 2, y];
            blacks[15] = _intPixels[x - 2, y - 1];
            blacks[16] = _intPixels[x - 4, y - 4];

            return blacks;
        }

        private static int[] GetArrayOfBlacksSquare9(int x, int y)
        {
            int[] blacks = new int[33];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = _intPixels[x - 4, y - 4];
            blacks[1] = _intPixels[x - 3, y - 4];
            blacks[2] = _intPixels[x - 2, y - 4];
            blacks[3] = _intPixels[x - 1, y - 4];
            blacks[4] = _intPixels[x, y - 4];
            blacks[5] = _intPixels[x + 1, y - 4];
            blacks[6] = _intPixels[x + 2, y - 4];
            blacks[7] = _intPixels[x + 3, y - 4];
            blacks[8] = _intPixels[x + 4, y - 4];
            blacks[9] = _intPixels[x + 4, y - 3];
            blacks[10] = _intPixels[x + 4, y - 2];
            blacks[11] = _intPixels[x + 4, y - 1];
            blacks[12] = _intPixels[x + 4, y];
            blacks[13] = _intPixels[x + 4, y + 1];
            blacks[14] = _intPixels[x + 4, y + 2];
            blacks[15] = _intPixels[x + 4, y + 3];
            blacks[16] = _intPixels[x + 4, y + 4];
            blacks[17] = _intPixels[x + 3, y + 4];
            blacks[18] = _intPixels[x + 2, y + 4];
            blacks[19] = _intPixels[x + 1, y + 4];
            blacks[20] = _intPixels[x, y + 4];
            blacks[21] = _intPixels[x - 1, y + 4];
            blacks[22] = _intPixels[x - 2, y + 4];
            blacks[23] = _intPixels[x - 3, y + 4];
            blacks[24] = _intPixels[x - 4, y + 4];
            blacks[25] = _intPixels[x - 4, y + 3];
            blacks[26] = _intPixels[x - 4, y + 2];
            blacks[27] = _intPixels[x - 4, y + 1];
            blacks[28] = _intPixels[x - 4, y];
            blacks[29] = _intPixels[x - 4, y - 1];
            blacks[30] = _intPixels[x - 4, y - 2];
            blacks[31] = _intPixels[x - 4, y - 3];
            blacks[32] = _intPixels[x - 4, y - 4];

            return blacks;
        }

        private static int[] GetArrayOfBlacksSquare3(int x, int y)
        {
            int[] blacks = new int[9];

            //if it's stupid but it works, it ain't stupid
            blacks[0] = _intPixels[x - 1, y - 1];
            blacks[1] = _intPixels[x, y - 1];
            blacks[2] = _intPixels[x + 1, y - 1];
            blacks[3] = _intPixels[x + 1, y];
            blacks[4] = _intPixels[x + 1, y + 1];
            blacks[5] = _intPixels[x, y + 1];
            blacks[6] = _intPixels[x - 1, y + 1];
            blacks[7] = _intPixels[x - 1, y];
            blacks[8] = _intPixels[x - 1, y - 1];

            return blacks;
        }
        private static void MarkEndings(int x, int y)
        {
            int counterForEnding = 0;
            bool square3 = false;
            int[] blacksInSquare3 = GetArrayOfBlacksSquare3(x, y);

            foreach (var pixel in blacksInSquare3)
            {
                if (pixel == 1)
                {
                    square3 = true;
                }
                if (pixel == 0 && square3)
                {
                    counterForEnding++;
                    square3 = false;
                }
            }

            Debug.WriteLine(counterForEnding);
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
