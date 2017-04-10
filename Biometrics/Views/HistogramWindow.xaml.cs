using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Biometrics.Classes;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : Window
    {
        public HistogramWindow(BitmapSource image)
        {
            InitializeComponent();
            ImageToHistogram.Source = image;
            ShowHistograms();
            InitYScale();
        }

        private void ShowHistograms()
        {
            //set points on polygons for visual representation of histograms
            HistogramUsredniony.Points = ConvertToPointCollection(HistogramTools.HistogramU);
            HistogramRed.Points = ConvertToPointCollection(HistogramTools.HistogramR);
            HistogramGreen.Points = ConvertToPointCollection(HistogramTools.HistogramG);
            HistogramBlue.Points = ConvertToPointCollection(HistogramTools.HistogramB);
        }

        //int[] table to Points Collection Converter, points are used for visual representation
        public static PointCollection ConvertToPointCollection(int[] values)
        {
            var max = values.Max();

            var points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (var i = 0; i < values.Length; i++)
                points.Add(new Point(i, max - values[i]));
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));

            return points;
        }

        private void InitYScale()
        {
            MaxHistogramValueAverage.Content = HistogramTools.HistogramU.Max();
            MaxHistogramValueRed.Content = HistogramTools.HistogramR.Max();
            MaxHistogramValueGreen.Content = HistogramTools.HistogramG.Max();
            MaxHistogramValueBlue.Content = HistogramTools.HistogramB.Max();
        }
    }
}