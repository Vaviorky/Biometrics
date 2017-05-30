using System;
using System.Collections.Generic;
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
        private static byte[] _pixels;
        private static int[,] _intPixels, _tempIntPixels;
        public static BitmapSource MarkSquares(BitmapSource source, int xPos, int yPos)
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

            for (int i= -2; i <= 2; i++)
            {
                for (int j = -2; j <= 2; j++)
                {
                    if (i != -2 && i != 2 && j != -2 && j != 2)
                        continue;

                    if (_intPixels[xPos + i, yPos + j] == 1)
                    {
                        continue;
                    }

                    _intPixels[xPos + i, yPos + j] = 2;
                }
            }

            for (int i = -4; i <= 4; i++)
            {
                for (int j = -4; j <= 4; j++)
                {
                    if (i != -4 && i != 4 && j != -4 && j != 4)
                        continue;

                    if (_intPixels[xPos + i, yPos + j] == 1)
                    {
                        continue;
                    }

                    _intPixels[xPos + i, yPos + j] = 3;
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

                }
            }
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
