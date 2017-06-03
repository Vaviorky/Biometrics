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
            int[] blacksInSquare5 = MinutiaeHelpers.GetArrayOfBlacksSquare5(x, y, _intPixels);
            int[] blacksInSquare9 = MinutiaeHelpers.GetArrayOfBlacksSquare9(x, y, _intPixels);

            foreach (var pixel in blacksInSquare5)
            {
                Debug.Write(pixel + " ");
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

            Debug.WriteLine("");

            Debug.WriteLine(countForSquare5 + " " + countForSquare9);
            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("");
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
                    else if (_pixels[j + 2] == 255 && _pixels[j + 1] == 0 && _pixels[j] == 0)
                    {
                        _intPixels[x, y] = 2;
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
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (_tempIntPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_tempIntPixels[x, y] == 3)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 0;
                    }

                }
            }
        }

        private static void MarkEndings(int x, int y)
        {
            int counterForEnding = 0;

            int[] blacksInSquare3 = MinutiaeHelpers.GetArrayOfBlacksSquare3(x, y, _intPixels);

            foreach (var pixel in blacksInSquare3)
            {
                bool square3 = false;

                if (pixel == 1)
                {
                    square3 = true;
                }
                if (pixel == 0 && square3)
                {
                    counterForEnding++;
                }
            }

            Debug.WriteLine(counterForEnding);
        }

        public static BitmapSource DeleteRepetations(BitmapSource source, int x, int y)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();
            _tempIntPixels = new int[_width, _height];
            for (int i = 0; i < _width; i++)
            {
                for (int j = 0; j < _height; j++)
                {
                    _tempIntPixels[i, j] = _intPixels[i, j];
                }
            }

            List<List<int[]>> minutiaesRepetationsList = new List<List<int[]>>();

            if (_intPixels[x, y] == 2)
            {
                List<int[]> minutiaesInSingleArea = new List<int[]>();
                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        // if it goes out of image
                        if (x + i < 0 || x + i >= _width || y + j < 0 || y + j >= _height /*|| (i == 0 && j == 0)*/)
                            continue;

                        if (_intPixels[x + i, y + j] == 2)
                        {
                            int posX = x + i;
                            int posY = y + j;

                            int[] pixel = new int[2];
                            pixel[0] = posX;
                            pixel[1] = posY;

                            minutiaesInSingleArea.Add(pixel);
                        }
                    }
                }

                foreach (var pixel in minutiaesInSingleArea)
                {
                    var tempX = pixel[0];
                    var tempY = pixel[1];

                    int redCount = 0;
                    if (_intPixels[tempX - 1, tempY] == 2)
                    {
                        redCount++;
                    }

                    if (_intPixels[tempX + 1, tempY] == 2)
                    {
                        redCount++;
                    }

                    if (_intPixels[tempX, tempY - 1] == 2)
                    {
                        redCount++;
                    }
                    if (_intPixels[tempX, tempY + 1] == 2)
                    {
                        redCount++;
                    }

                    if (redCount < 2)
                    {
                        Debug.WriteLine(tempX + " " + tempY + " " + _intPixels[tempX, tempY]);
                        _tempIntPixels[tempX, tempY] = 1;
                    }

                    if (redCount == 2)
                    {
                        Debug.WriteLine("RED FOREVER");
                        _tempIntPixels[tempX, tempY] = 2;
                    }

                }


                for (int i = -2; i <= 2; i++)
                {
                    for (int j = -2; j <= 2; j++)
                    {
                        // if it goes out of image
                        if (x + i < 0 || x + i >= _width || y + j < 0 || y + j >= _height)
                            continue;

                        //_intPixels[x + i, y + j] = _tempIntPixels[x + i, y + j];
                    }
                }

                //_intPixels[tempX, tempY] = 3;

            }

            RevertIntPixelsIntoPixelArray();
            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
