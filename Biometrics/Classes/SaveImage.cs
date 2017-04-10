using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Biometrics.Classes
{
    public static class SaveImage
    {
        public static int X { get; set; }
        public static int Y { get; set; }

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
                encoder.Frames.Add(CreateResizedImage(source,X,Y,0));
                encoder.Save(fileStream);
            }
        }

        private static BitmapFrame CreateResizedImage(BitmapSource source, int width, int height, int margin)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(group);
            }

            var resizedImage = new RenderTargetBitmap(
                width, height, // Resized dimensions
                96, 96, // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }
    }
}