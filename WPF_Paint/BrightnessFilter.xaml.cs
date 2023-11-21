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
    public partial class BrightnessFilter : Window
    {
        public double Value { get; private set; } = 0;

        public BrightnessFilter()
        {
            InitializeComponent();
            RedTextBox.Text = "0";
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            double result;
            if (double.TryParse(RedTextBox.Text, out result) && result>=-255 && result<=255)
            {
                Value = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number.");
                return;
            }
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

    }
}
