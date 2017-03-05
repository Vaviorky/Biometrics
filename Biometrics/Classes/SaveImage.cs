using System.IO;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class SaveImage
    {
        public static void SaveToBmp(BitmapSource visual, string fileName)
        {
            var encoder = new BmpBitmapEncoder();
            SaveImgToFile(fileName, encoder, visual);
        }

        public static void SaveToPng(BitmapSource visual, string fileName)
        {
            var encoder = new PngBitmapEncoder();
            SaveImgToFile(fileName, encoder, visual);
        }

        public static void SaveToJpeg(BitmapSource visual, string fileName)
        {
            var encoder = new JpegBitmapEncoder();
            SaveImgToFile(fileName, encoder, visual);
        }

        public static void SaveToGif(BitmapSource visual, string fileName)
        {
            var encoder = new JpegBitmapEncoder();
            SaveImgToFile(fileName, encoder, visual);
        }

        public static void SaveToTiff(BitmapSource visual, string fileName)
        {
            var encoder = new TiffBitmapEncoder();
            SaveImgToFile(fileName, encoder, visual);
        }

        private static void SaveImgToFile(string filePath, BitmapEncoder encoder, BitmapSource source)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(fileStream);
            }
        }
    }
}