using System;
using System.Collections.Generic;
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
using WPF_Paint.ViewModels;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class PointFilter : Window
    {
        public double[] Value { get; private set; } = new double[3];

        public PointFilter(string title)
        {
            InitializeComponent();
            this.Title = title;
            RedTextBox.Text = "0";
            GreenTextBox.Text = "0";
            BlueTextBox.Text = "0";
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            double result;
            if (double.TryParse(RedTextBox.Text, out result) && result>=0 && result<=255)
            {
                Value[0] = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Red).");
                return;
            }
            if (double.TryParse(GreenTextBox.Text, out result) && result >= 0 && result <= 255)
            {
                Value[1] = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Green).");
                return;
            }
            if (double.TryParse(BlueTextBox.Text, out result) && result >= 0 && result <= 255)
            {
                Value[2] = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Blue).");
                return;
            }

            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

    }
}
