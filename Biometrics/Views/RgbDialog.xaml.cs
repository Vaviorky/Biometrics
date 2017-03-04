using System.Windows;
using System.Windows.Media;

namespace Biometrics.Views
{
    /// <summary>
    ///     Interaction logic for RgbDialog.xaml
    /// </summary>
    public partial class RgbDialog : Window
    {
        private Point _mousePoint;
        private byte _r, _g, _b;

        public RgbDialog()
        {
            InitializeComponent();
        }

        public RgbDialog(byte r, byte g, byte b, Point mousePoint)
        {
            InitializeComponent();
            Left = mousePoint.X;
            Top = mousePoint.Y;
            _r = r;
            _g = g;
            _b = b;
            _mousePoint = mousePoint;

            RectangleColor.Fill = new SolidColorBrush(Color.FromRgb(_r,_g,_b));

            RLabel.Text = _r.ToString();
            GLabel.Text = _g.ToString();
            BLabel.Text = _b.ToString();


        }

        private void RgbButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}