using System;
using System.Windows;
using System.Windows.Input;
using Biometrics.Classes;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for ManualMask.xaml
    /// </summary>
    public partial class ManualMask : Window
    {
        private readonly int[,] _maskTable3X3 = new int[3, 3];
        private readonly int[,] _maskTable5X5 = new int[5, 5];

        public ManualMask()
        {
            InitializeComponent();
        }

        private void ToggleButton3x3_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Grid3X3.Visibility = Visibility.Visible;
                Grid5X5.Visibility = Visibility.Hidden;
                Width = 300;
                Height = 300;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void ToggleButton5x5_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                Grid3X3.Visibility = Visibility.Hidden;
                Grid5X5.Visibility = Visibility.Visible;
                Width = 400;
                Height = 400;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void KeyDownTextBox(object sender, KeyEventArgs e)
        {
            if (IsKeyDigit(e.Key) || e.Key == Key.Back || e.Key == Key.OemMinus)
            {
                //Digits, backspace key are allowed
            }
            else
            {
                //don't allow other keys
                e.Handled = true;
            }
        }

        private static bool IsKeyDigit(Key key)
        {
            return key >= Key.D0 && key <= Key.D9;
        }

        private void ButtonOK_OnClick(object sender, RoutedEventArgs e)
        {
            GetMaskTableInfo();
            
            if (Grid3X3.IsVisible)
                MainWindow.ModifiedImgSingleton.Source = Mask.PerformMask(_maskTable3X3, 3);

            if (Grid5X5.IsVisible)
                MainWindow.ModifiedImgSingleton.Source = Mask.PerformMask(_maskTable5X5, 5);
            Close();
        }

        private void GetMaskTableInfo()
        {
            if (Grid3X3.IsVisible)
            {
                _maskTable3X3[0, 0] = Convert.ToInt32(TextBox3X31X1.Text);
                _maskTable3X3[0, 1] = Convert.ToInt32(TextBox3X31X2.Text);
                _maskTable3X3[0, 2] = Convert.ToInt32(TextBox3X31X3.Text);

                _maskTable3X3[1, 0] = Convert.ToInt32(TextBox3X32X1.Text);
                _maskTable3X3[1, 1] = Convert.ToInt32(TextBox3X32X2.Text);
                _maskTable3X3[1, 2] = Convert.ToInt32(TextBox3X32X3.Text);

                _maskTable3X3[2, 0] = Convert.ToInt32(TextBox3X33X1.Text);
                _maskTable3X3[2, 1] = Convert.ToInt32(TextBox3X33X2.Text);
                _maskTable3X3[2, 2] = Convert.ToInt32(TextBox3X33X3.Text);
            }
            if (Grid5X5.IsVisible)
            {
                _maskTable5X5[0, 0] = Convert.ToInt32(TextBox5X51X1.Text);
                _maskTable5X5[0, 1] = Convert.ToInt32(TextBox5X51X2.Text);
                _maskTable5X5[0, 2] = Convert.ToInt32(TextBox5X51X3.Text);
                _maskTable5X5[0, 3] = Convert.ToInt32(TextBox5X51X4.Text);
                _maskTable5X5[0, 4] = Convert.ToInt32(TextBox5X51X5.Text);

                _maskTable5X5[1, 0] = Convert.ToInt32(TextBox5X52X1.Text);
                _maskTable5X5[1, 1] = Convert.ToInt32(TextBox5X52X2.Text);
                _maskTable5X5[1, 2] = Convert.ToInt32(TextBox5X52X3.Text);
                _maskTable5X5[1, 3] = Convert.ToInt32(TextBox5X52X4.Text);
                _maskTable5X5[1, 4] = Convert.ToInt32(TextBox5X52X5.Text);

                _maskTable5X5[2, 0] = Convert.ToInt32(TextBox5X53X1.Text);
                _maskTable5X5[2, 1] = Convert.ToInt32(TextBox5X53X2.Text);
                _maskTable5X5[2, 2] = Convert.ToInt32(TextBox5X53X3.Text);
                _maskTable5X5[2, 3] = Convert.ToInt32(TextBox5X53X4.Text);
                _maskTable5X5[2, 4] = Convert.ToInt32(TextBox5X53X5.Text);

                _maskTable5X5[3, 0] = Convert.ToInt32(TextBox5X54X1.Text);
                _maskTable5X5[3, 1] = Convert.ToInt32(TextBox5X54X2.Text);
                _maskTable5X5[3, 2] = Convert.ToInt32(TextBox5X54X3.Text);
                _maskTable5X5[3, 3] = Convert.ToInt32(TextBox5X54X4.Text);
                _maskTable5X5[3, 4] = Convert.ToInt32(TextBox5X54X5.Text);

                _maskTable5X5[4, 0] = Convert.ToInt32(TextBox5X55X1.Text);
                _maskTable5X5[4, 1] = Convert.ToInt32(TextBox5X55X2.Text);
                _maskTable5X5[4, 2] = Convert.ToInt32(TextBox5X55X3.Text);
                _maskTable5X5[4, 3] = Convert.ToInt32(TextBox5X55X4.Text);
                _maskTable5X5[4, 4] = Convert.ToInt32(TextBox5X55X5.Text);
            }
        }
    }
}