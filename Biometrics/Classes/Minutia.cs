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
        private static int[,] _intPixels;
        private static List<int[]> _minutiaCandidates;
        public static BitmapSource MarkMinuties(BitmapSource source)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            _minutiaCandidates = new List<int[]>();

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);
            int count3, count5;
            bool square3WithCount3, square5WithCount3;
            InitIntImage();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {

                    if (_intPixels[x, y] != 1)
                    {
                        continue;
                    }

                    count3 = 0;
                    count5 = 0;
                    square3WithCount3 = false;
                    square5WithCount3 = false;

                    //square a = 5
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            //check if i,j in border of sqare (a==5)
                            if (i != -1 && i != 1 && j != -1 && j != 1)
                                continue;

                            if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                                continue;

                            if (_intPixels[x + j, y + i] == 1)
                                count3++;

                        }
                    }

                    if (count3 == 3)
                    {
                        _intPixels[x, y] = 2;
                    }



                    /* //square a = 9
                     for (int i = -4; i <= 4; i++)
                     {
                         for (int j = -4; j <= 4; j++)
                         {
                             //check if i,j in border of sqare (a==5)
                             if (i != -4 && i != 4 && j != -4 && j != 4)
                                 continue;

                             if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                                 continue;

                             if (_intPixels[x + j, y + i] == 1)
                                 count5++;

                             if (count5 == 3)
                                 square5WithCount3 = true;

                         }
                     }

                     //if both squares are 
                     if (square3WithCount3 && square5WithCount3)
                     {
                         _minutiaCandidates.Add(new[]
                         {
                             x,y
                         });
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

                }
            }
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
