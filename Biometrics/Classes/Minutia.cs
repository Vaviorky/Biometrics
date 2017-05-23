using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class Minutia
    {

        private static int width, height, stride;
        private static byte[] pixels;
        public static BitmapSource MarkMinuties(BitmapSource source)
        {
            WriteableBitmap bitmap = new WriteableBitmap(source);

            width = bitmap.PixelWidth;
            height = bitmap.PixelHeight;

            stride = width * ((bitmap.Format.BitsPerPixel + 7) / 8);

            var arraySize = stride * height;
            pixels = new byte[arraySize];

            //copy all data about pixels values into 1-dimentional array
            bitmap.CopyPixels(pixels, stride, 0);


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    
                }
            }


            var rect = new Int32Rect(0, 0, width, height);
            bitmap.WritePixels(rect, pixels, stride, 0);
            return bitmap;
        }
    }
}
