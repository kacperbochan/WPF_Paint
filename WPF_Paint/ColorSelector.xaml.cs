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
    public partial class ColorSelector : Window
    {
        public ColorSelector()
        {
            InitializeComponent();
            DataContext = new ViewModelColors();
        }

        public ViewModelColors ViewModel
        {
            get { return DataContext as ViewModelColors; }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Pobierz wartości RGB z ViewModel i przekształć je na byte
            byte redValue = byte.Parse(ViewModel.RedValue);
            byte greenValue = byte.Parse(ViewModel.GreenValue);
            byte blueValue = byte.Parse(ViewModel.BlueValue);

            // Utwórz kolor na podstawie wartości RGB
            Color selectedColor = Color.FromRgb(redValue, greenValue, blueValue);

            // Wywołaj zdarzenie z wybranym kolorem
            ViewModel.OnColorSelected(selectedColor);

            // Zamknij okno
            this.Close();
        }

    }
}
