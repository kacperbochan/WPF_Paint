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

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for FilterValues.xaml
    /// </summary>
    public partial class FilterValues : Window
    {
        public double[,] Values;

        public FilterValues(int width, int height)
        {
            InitializeComponent();
            GenerateTextBoxes(width, height);
            Values = new double[width, height];
        }

        public void GenerateTextBoxes(int width, int height)
        {
            DynamicGrid.Children.Clear();
            DynamicGrid.RowDefinitions.Clear();
            DynamicGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < height; i++)
            {
                DynamicGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int j = 0; j < width; j++)
            {
                DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    TextBox textBox = new TextBox
                    {
                        Text = "0" // Set initial value to '0'
                    };
                    Grid.SetRow(textBox, i);
                    Grid.SetColumn(textBox, j);
                    DynamicGrid.Children.Add(textBox);
                }
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateAndStoreValues())
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // Wyświetl powiadomienie o błędzie
                MessageBox.Show("Wprowadź poprawne wartości liczbowe.");
            }
        }

        private bool ValidateAndStoreValues()
        {
            int rows = DynamicGrid.RowDefinitions.Count;
            int cols = DynamicGrid.ColumnDefinitions.Count;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    TextBox textBox = DynamicGrid.Children
                        .Cast<UIElement>()
                        .First(e => Grid.GetRow(e) == i && Grid.GetColumn(e) == j) as TextBox;

                    if (textBox != null && double.TryParse(textBox.Text, out double value))
                    {
                        Values[j, i] = value;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
