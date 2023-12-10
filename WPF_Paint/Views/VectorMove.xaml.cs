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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace WPF_Paint.Views
{
    /// <summary>
    /// Logika interakcji dla klasy VectorMove.xaml
    /// </summary>
    public partial class VectorMove : Window
    {
        public int X = 3;
        public int Y = 3;

        public VectorMove(int x, int y)
        {
            InitializeComponent();
            XTextBox.Text = x.ToString();
            YTextBox.Text = y.ToString();
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
                MessageBox.Show("Please enter a valid number (Width).");
                return;
            }
            if (int.TryParse(YTextBox.Text, out result))
            {
                Y = result;
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
