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

namespace WPF_Paint.Views
{

    public partial class RotateMove : Window
    {
        public int X = 0;
        public int Y = 0;
        public int Angle = 0;

        public RotateMove(int x, int y, int angle)
        {
            InitializeComponent();
            XTextBox.Text = x.ToString();
            YTextBox.Text = y.ToString();
            AngleTextBox.Text = angle.ToString();
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
            if (int.TryParse(AngleTextBox.Text, out result) && result < 360 && result>0)
            {
                Angle = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid number (Angle).");
                return;
            }

            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }
    }
}
