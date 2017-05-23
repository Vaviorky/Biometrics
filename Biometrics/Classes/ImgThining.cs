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
    public static class ImgThining
    {
        private enum Color
        {
            White,
            Black,
            Red,
            Green,
            Magenta
        }

        private static byte[] _pixels;
        private static int _stride;
        private static int _width, _height;
        private static int[,] intPixels;

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

        //0
        private static readonly int[] A0 =
        {
            3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56, 60,
            62, 63, 96, 112, 120, 124, 126, 127, 129, 131,
            135, 143, 159, 191, 192, 193, 195, 199, 207, 223,
            224, 225, 227, 231, 239, 240, 241, 243, 247, 248,
            249, 251, 252, 253, 254
        };


        //1
        private static readonly int[] A1 =
        {
            7, 14, 28, 56, 112, 131, 193, 224
        };

        //2
        private static readonly int[] A2 =
        {
            7, 14, 15, 28, 30, 56, 60, 112, 120, 131, 135,
            193, 195, 224, 225, 240
        };

        //3
        private static readonly int[] A3 =
        {
            7, 14, 15, 28, 30, 31, 56, 60, 62, 112, 120, 124,
            131, 135, 143, 193, 195, 199, 224, 225, 227, 240,
            241, 248
        };

        //4
        private static readonly int[] A4 =
        {
            7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
            124, 126, 131, 135, 143, 159, 193, 195, 199, 207,
            224, 225, 227, 231, 240, 241, 243, 248, 249, 252
        };

        //5
        private static readonly int[] A5 =
        {
            7, 14, 15, 28, 30, 31, 56, 60, 62, 63, 112, 120,
            124, 126, 131, 135, 143, 159, 191, 193, 195, 199,
            207, 224, 225, 227, 231, 239, 240, 241, 243, 248,
            249, 251, 252, 254
        };

        //finish
        private static readonly int[] A1Pix =
        {
            3, 6, 7, 12, 14, 15, 24, 28, 30, 31, 48, 56,
            60, 62, 63, 96, 112, 120, 124, 126, 127, 129, 131,
            135, 143, 159, 191, 192, 193, 195, 199, 207, 223,
            224, 225, 227, 231, 239, 240, 241, 243, 247, 248,
            249, 251, 252, 253, 254
        };


        public static BitmapSource ThinImage(BitmapSource original)
        {
            var bitmap = new WriteableBitmap(original);

            _width = bitmap.PixelWidth;
            _height = bitmap.PixelHeight;

            _stride = _width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * _height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            InitIntImage();

            bool thinned;
            do
            {
                thinned = true;

                //phase zero, mark "two"
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (intPixels[x, y] == 1 && Weight(x, y, A0))
                        {
                            SetPixel(x, y, Color.Red);
                        }
                    }

                }

                //phase one, delete with 3 neighours
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)) && Weight(x, y, A1))
                        {
                            Delete(x, y);
                            thinned = false;
                        }
                    }
                }

                //phase two, delete with 3 or 4 neighours
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)) && Weight(x, y, A2))
                        {
                            Delete(x, y);
                            thinned = false;
                        }
                    }
                }

                //phase three, delete with 3 or 4 or 5 neighours
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)) && Weight(x, y, A3))
                        {
                            Delete(x, y);
                            thinned = false;
                        }
                    }
                }

                //phase four, delete with 3 or 4 or 5 or 6 neighours
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)) && Weight(x, y, A4))
                        {
                            Delete(x, y);
                            thinned = false;
                        }
                    }
                }

                //phase five, delete with 3 or 4 or 5 or 6 or 7 neighours
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)) && Weight(x, y, A5))
                        {
                            Delete(x, y);
                            thinned = false;
                        }
                    }
                }

                //red back to black
                for (int x = 0; x < _width; x++)
                {
                    for (int y = 0; y < _height; y++)
                    {
                        if (IsColor(Color.Red, C(x, y)))
                        {
                            SetPixel(x, y, Color.Black);
                        }
                    }
                }


            } while (!thinned);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (IsColor(Color.Black, C(x, y)) && Weight(x, y, A1Pix))
                    {
                        Delete(x, y);
                    }
                }
            }

            RevertIntPixelsIntoPixelArray();

            var rect = new Int32Rect(0, 0, _width, _height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
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

        private static int NW(int x, int y)
        {
            return GetPixel(x - 1, y - 1);
        }

        private static int N(int x, int y)
        {
            return GetPixel(x, y - 1);
        }

        private static int NE(int x, int y)
        {
            return GetPixel(x + 1, y - 1);
        }

        private static int E(int x, int y)
        {
            return GetPixel(x + 1, y);
        }

        private static int SE(int x, int y)
        {
            return GetPixel(x + 1, y + 1);
        }

        private static int S(int x, int y)
        {
            return GetPixel(x, y + 1);
        }

        private static int SW(int x, int y)
        {
            return GetPixel(x - 1, y + 1);
        }

        private static int W(int x, int y)
        {
            return GetPixel(x - 1, y);
        }

        private static int C(int x, int y)
        {
            return GetPixel(x, y);
        }

        private static int GetPixel(int x, int y)
        {
            int j = PositionInArray(x, y);

            if (j < 0 || y > _height - 1 || x > _width - 1)
            {
                return -1;
            }

            return intPixels[x, y];
        }

        private static void SetPixel(int x, int y, Color color)
        {
            switch (color)
            {
                case Color.White:
                    intPixels[x, y] = 0;
                    break;

                case Color.Black:
                    intPixels[x, y] = 1;
                    break;

                case Color.Red:
                    intPixels[x, y] = 2;
                    break;
            }
        }

        private static bool IsColor(Color color, int pixel)
        {
            if (color == Color.White && pixel == 0)
            {
                return true;
            }

            if (color == Color.Black && pixel == 1)
            {
                return true;
            }

            if (color == Color.Red && pixel == 2)
            {
                return true;
            }

            return false;
        }

        private static bool Weight(int x, int y, int[] array)
        {
            int weight = 1 * (N(x, y) % 2) + 2 * (NE(x, y) % 2) + 4 * (E(x, y) % 2) + 8 * (SE(x, y) % 2)
                + 16 * (S(x, y) % 2) + 32 * (SW(x, y) % 2) + 64 * (W(x, y) % 2) + 128 * (NW(x, y) % 2);

            return array.Contains(weight);
        }

        //white
        private static void Delete(int x, int y)
        {
            SetPixel(x, y, Color.White);
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }
    }
}