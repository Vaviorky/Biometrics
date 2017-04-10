using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Biometrics.Classes;

namespace Biometrics.Views
{
    /// <summary>
    /// Interaction logic for Niblack.xaml
    /// </summary>
    public partial class Niblack : Window
    {
        private double _parameter;
        private int _size;
        private Progress<int> _progress;
        public Niblack()
        {
            InitializeComponent();
            InitProgressBar();
        }

        private async void NiblackOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _size = Convert.ToInt32(WindowSize.Text);
                _parameter = Convert.ToDouble(TresholdingParameter.Text, CultureInfo.InvariantCulture);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace + " " + exception.Message);
            }

            WindowSize.IsEnabled = false;
            TresholdingParameter.IsEnabled = false;
            DoNiblackButton.IsEnabled = false;
            var b = new Binarization();
            
           MainWindow.ModifiedImgSingleton.Source =  await b.PerformNiblack(_parameter, _size, _progress);
            Close();
        }

        private void InitProgressBar()
        {
            var bitmap = (BitmapSource) MainWindow.ModifiedImgSingleton.Source;
            ProgressBar.Maximum = (bitmap.PixelWidth * bitmap.PixelHeight)/2 ;
            _progress = new Progress<int>(value =>
            {
                ProgressBar.Value++;
            });
        }
    }
}
