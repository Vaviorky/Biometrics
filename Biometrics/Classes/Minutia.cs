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
        private static int[,] _intPixels, _tempIntPixels;

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

            InitIntImage();

            _tempIntPixels = _intPixels;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {

                    if (_intPixels[x, y] != 1)
                    {
                        continue;
                    }

                    int count3 = 0;
                    int count5 = 0;
                    bool square3WithCount3 = false;
                    bool square5WithCount3 = false;

                    //square a = 5
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            //check if i,j in border of sqare (a==5)
                            if (i != -2 && i != 2 && j != -2 && j != 2)
                                continue;

                            if (x + j < 0 || x + j >= _width || y + i < 0 || y + i >= _height || (i == 0 && j == 0))
                                continue;

                            if (_intPixels[x + j, y + i] == 1)
                                count3++;
                            
                        }
                    }

                    if (count3 == 3)
                        square3WithCount3 = true;
                    else
                        square3WithCount3 = false;

                    //square a = 9
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

                        }
                    }

                    if (count5 == 3)
                        square5WithCount3 = true;
                    else
                    {
                        square5WithCount3 = false;
                    }

                    //if both squares are 
                    if (square3WithCount3 && square5WithCount3)
                    {
                        _minutiaCandidates.Add(new[]
                        {
                             x,y
                         });
                    }

                }
            }

            MarkPotentialMinutiaes();


            RevertIntPixelsIntoPixelArray();


            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        private static void MarkPotentialMinutiaes()
        {
            foreach (var minutiaCandidate in _minutiaCandidates)
            {
                var x = minutiaCandidate[0];
                var y = minutiaCandidate[1];

                Debug.WriteLine(x + " " + y);

                _tempIntPixels[x, y] = 2;
            }
        }

        //another method...
        private static float GetCnValue(int x, int y)
        {
            //ignore the boundaries cause why not
            if (x == 0 || y == 0 || x == _width - 1 || y == _height - 1)
            {
                return -1f;
            }

            int[] P =
            {
                _intPixels[x + 1, y],           //P1 = P9
                _intPixels[x + 1, y - 1],       //P2
                _intPixels[x, y + 1],           //P3
                _intPixels[x - 1, y + 1],       //P4
                _intPixels[x - 1, y],           //P5
                _intPixels[x - 1, y - 1],       //P6
                _intPixels[x, y - 1],           //P7
                _intPixels[x + 1, y - 1]        //P8
            };

            int cnValue = 0;

            for (int i = 0; i < P.Length - 2; i++)
            {
                //P1..P8
                cnValue += Math.Abs(P[i] - P[i + 1]);
            }

            //P8..P9
            cnValue += Math.Abs(P[7] - P[0]);

            cnValue = cnValue / 2;

            return cnValue;

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

                }
            }
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}
