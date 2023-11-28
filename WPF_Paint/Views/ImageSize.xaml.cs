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
    public partial class ImageSize : Window
    {
        public int Width=3;
        public int Height=3;

        public double[,] Values { get; private set; } = new double[3,3];

        public ImageSize(int width, int height)
        {
            InitializeComponent();
            WidthTextBox.Text = width.ToString();
            HeightTextBox.Text = height.ToString();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            int result;
            if (int.TryParse(WidthTextBox.Text, out result) && result >= 0) 
            {
                Width = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Width).");
                return;
            }
            if (int.TryParse(HeightTextBox.Text, out result) && result >= 0)
            {
                Height = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Height).");
                return;
            }

            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

    }
}
