using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

namespace WPF_Paint.Views
{
    /// <summary>
    /// Logika interakcji dla klasy ChangeScale.xaml
    /// </summary>
    public partial class ChangeScale : Window
    {
        public int X = 0;
        public int Y = 0;
        public double ScaleFactor = 1;
        public ChangeScale(int x, int y, double scaleFactor)
        {
            InitializeComponent();
            InitializeComponent();
            XTextBox.Text = x.ToString();
            YTextBox.Text = y.ToString();
            ScaleFactorTextBox.Text = scaleFactor.ToString();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            int result;
            if (int.TryParse(XTextBox.Text, out result))
            {
                X = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (X).");
                return;
            }
            if (int.TryParse(YTextBox.Text, out result))
            {
                Y = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Y).");
                return;
            }
            if (double.TryParse(ScaleFactorTextBox.Text, out double scaleResult))
            {
                ScaleFactor = scaleResult;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Size).");
                return;
            }

            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }
    }
}
