using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Biometrics.Classes;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for BinarisationOwnTreshold.xaml
    /// </summary>
    public partial class BinarisationOwnTreshold : Window
    {
        public BinarisationOwnTreshold()
        {
            InitializeComponent();
            PreviewImageUpdate(125);

        }

        private void ThesholdSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var newValue = (int) e.NewValue;
            TresholdTextBox.Text = newValue.ToString();
        }

        private void TreshloldButtonOkClick(object sender, RoutedEventArgs e)
        {
            Close();

            MainWindow.ModifiedImgSingleton.Source =
                Binarization.ManualBinarisation((BitmapSource) MainWindow.ModifiedImgSingleton.Source,
                    (int) ThesholdSlider.Value);
        }

        private void TreshholdButtonCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TresholdTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ThesholdSlider.Value = Convert.ToDouble(TresholdTextBox.Text);
                PreviewImageUpdate((int) ThesholdSlider.Value);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.StackTrace);
            }
        }

        private void PreviewImageUpdate(int treshlold)
        {
            try
            {
                PreviewImage.Source = Binarization.ManualBinarisation((BitmapSource)MainWindow.ModifiedImgSingleton.Source, treshlold);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}