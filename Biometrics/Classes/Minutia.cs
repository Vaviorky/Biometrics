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
    public static class Minutia
    {
        private static int _width, _height, _stride;
        private static byte[] _pixels;
        private static int[,] _intPixels, _bifurcationIntPixels, _endingIntPixels;

        public static BitmapSource MarkMinuties(BitmapSource source)
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

            _bifurcationIntPixels = _endingIntPixels = _intPixels;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int countForSquare5 = 0, countForSquare9 = 0;
                    bool square5 = false, square9 = false;

                    if (_intPixels[x, y] != 1)
                    {
                        continue;
                    }

                    int[] blacksInSquare5 = MinutiaeHelpers.GetArrayOfBlacksSquare5(x, y, _intPixels);
                    int[] blacksInSquare9 = MinutiaeHelpers.GetArrayOfBlacksSquare9(x, y, _intPixels);

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



                    //MarkEndings(x, y);

                    //if both squares are crossovers exactly 3 times
                    if (countForSquare5 == 3 && countForSquare9 == 3)
                    {
                        _bifurcationIntPixels[x, y] = 2;
                    }

                    /*if (countForSquare5 == 1 && countForSquare9 == 1)
                    {
                        _bifurcationIntPixels[x, y] = 3;
                    }*/



                }
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

                    if (_bifurcationIntPixels[x, y] == 0 || _endingIntPixels[x, y] == 0)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 255;
                        _pixels[j] = 255;
                    }

                    if (_bifurcationIntPixels[x, y] == 1 || _endingIntPixels[x, y] == 1)
                    {
                        _pixels[j + 2] = 0;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_bifurcationIntPixels[x, y] == 2 || _endingIntPixels[x, y] == 2)
                    {
                        _pixels[j + 2] = 255;
                        _pixels[j + 1] = 0;
                        _pixels[j] = 0;
                    }

                    if (_bifurcationIntPixels[x, y] == 3 || _endingIntPixels[x, y] == 3)
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
            bool square3 = false;

            int[] blacksInSquare3 = MinutiaeHelpers.GetArrayOfBlacksSquare3(x, y, _intPixels);

            foreach (var pixel in blacksInSquare3)
            {
                if (pixel == 1)
                {
                    square3 = true;
                }
                if (pixel == 0 && square3)
                {
                    counterForEnding++;
                    
                }
                square3 = false;
            }

            if (counterForEnding == 1)
            {
                _endingIntPixels[x, y] = 3;
            }
        }


        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
