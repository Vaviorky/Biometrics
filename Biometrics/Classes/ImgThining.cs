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

        public static bool IsImageBinarizated(BitmapSource image)
        {
            var bitmap = new WriteableBitmap(image);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            _stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);

            var j = 0;
            //count occurences of each intensity value which occur in image and store it
            for (var i = 0; i < _pixels.Length / 4; i++)
            {
                //get pixels channels values
                var r = _pixels[j + 2];
                var g = _pixels[j + 1];
                var b = _pixels[j];

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

        private static bool[] BuildKillsArray(int[] kills)
        {
            bool[] ar = new bool[256];

            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = false;
            }

            for (int i = 0; i < kills.Length; ++i)
                ar[kills[i]] = true;
            return ar;
        }

        private static readonly bool[] KillsRound = BuildKillsArray(new int[]
        {
            3, 12, 48, 192, 6, 24, 96, 129, //	-	2 s�siad�w
            14, 56, 131, 224, 7, 28, 112, 193, //	-	3 s�siad�w
            195, 135, 15, 30, 60, 120, 240, 225, //	-	4 s�siad�w
//			31, 62, 124, 248, 241, 227, 199, 143,//	-	5 s�siad�w
//			63, 126, 252, 249, 243, 231, 207, 159,//-	6 s�siad�w
//			254, 253, 251, 247, 239, 223, 190, 127,//-	7 s�siad�w
        });

        private static bool CanRound(int weight)
        {
            return KillsRound[weight];
        }

        private static readonly bool[] KillsFinish = BuildKillsArray(new int[]
        {
            5, 13, 15, 20, 21, 22, 23, 29,
            30, 31, 52, 53, 54, 55, 60, 61,
            62, 63, 65, 67, 69, 71, 77, 79,
            80, 81, 83, 84, 85, 86, 87, 88,
            89, 91, 92, 93, 94, 95, 97, 99,
            101, 103, 109, 111, 113, 115, 116, 117,
            118, 119, 120, 121, 123, 124, 125, 126,
            127, 133, 135, 141, 143, 149, 151, 157,
            159, 181, 183, 189, 191, 195, 197, 199,
            205, 207, 208, 209, 211, 212, 213, 214,
            215, 216, 217, 219, 220, 221, 222, 223,
            225, 227, 229, 231, 237, 239, 240, 241,
            243, 244, 245, 246, 247, 248, 249, 251,
            252, 253, 254, 255,

            3, 12, 48, 192, //	-	2 s�siad�w
            14, 56, 131, 224, //	-	3 s�siad�w 'I'
            7, 28, 112, 193, //	-	3 s�siad�w 'L'
        });

        private static bool CanKill(int weight)
        {
            return KillsFinish[weight];
        }

        public static BitmapSource ThinImage(BitmapSource original)
        {
            var bitmap = new WriteableBitmap(original);

            var width = bitmap.PixelWidth;
            var height = bitmap.PixelHeight;

            _stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = _stride * height;
            _pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(_pixels, _stride, 0);
            bool thinned;
            do
            {
                thinned = true;

                //BORDER
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (IsColor(Color.White, N(x, y)) || IsColor(Color.White, S(x, y)) ||
                            IsColor(Color.White, W(x, y)) || IsColor(Color.White, S(x, y)))
                        {
                            SetPixel(x, y, Color.Red);
                        }
                        else if (IsColor(Color.White, NW(x, y)) || IsColor(Color.White, SW(x, y)) ||
                                 IsColor(Color.White, SE(x, y)) || IsColor(Color.White, NE(x, y)))
                        {
                            SetPixel(x, y, Color.Magenta);
                        }
                    }
                }
            } while (!thinned);

            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, _pixels, _stride, 0);
            return bitmap;
        }

        private static int PositionInArray(int x, int y)
        {
            return 4 * x + y * _stride;
        }

        private static byte[] NW(int x, int y)
        {
            return GetPixel(x - 1, y - 1);
        }

        private static byte[] N(int x, int y)
        {
            return GetPixel(x, y - 1);
        }

        private static byte[] NE(int x, int y)
        {
            return GetPixel(x + 1, y - 1);
        }

        private static byte[] E(int x, int y)
        {
            return GetPixel(x + 1, y);
        }

        private static byte[] SE(int x, int y)
        {
            return GetPixel(x + 1, y + 1);
        }

        private static byte[] S(int x, int y)
        {
            return GetPixel(x, y + 1);
        }

        private static byte[] SW(int x, int y)
        {
            return GetPixel(x - 1, y + 1);
        }

        private static byte[] W(int x, int y)
        {
            return GetPixel(x - 1, y);
        }

        private static byte[] GetPixel(int x, int y)
        {
            int j = PositionInArray(x, y);
            if (j < 0)
            {
                return null;
            }
            byte[] pixel = {_pixels[j + 2], _pixels[j + 1], _pixels[j]};
            return pixel;
        }

        private static void SetPixel(int x, int y, Color color)
        {
            int j = PositionInArray(x, y);

            switch (color)
            {
                case Color.White:
                    _pixels[j + 2] = 255;
                    _pixels[j + 1] = 255;
                    _pixels[j] = 255;
                    break;
                case Color.Red:
                    _pixels[j + 2] = 255;
                    _pixels[j + 1] = 0;
                    _pixels[j] = 0;
                    break;

                case Color.Magenta:
                    _pixels[j + 2] = 255;
                    _pixels[j + 1] = 0;
                    _pixels[j] = 255;
                    break;
            }
        }

        private static bool IsColor(Color color, byte[] pixel)
        {
            if (pixel == null)
            {
                return false;
            }

            if (color == Color.White && pixel[0] == 255 && pixel[1] == 255 && pixel[2] == 255)
            {
                return true;
            }

            return false;
        }
    }
}