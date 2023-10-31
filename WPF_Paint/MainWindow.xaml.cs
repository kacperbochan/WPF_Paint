using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF_Paint.ViewModels;
using static WPF_Paint.HelperMethods;

namespace WPF_Paint
{
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
            MainCanvas.Background = Brushes.White;
            DataContext = new ViewModelBase();

            var viewModel = DataContext as ViewModelBase;
            if (viewModel != null)
            {
                viewModel.MainCanvas = MainCanvas;
            }
        }

        private ViewModelColors ViewModel
        {
            get { return DataContext as ViewModelColors; }
        }


        private void OpenColorSelector()
        {
            ColorSelector colorSelectorWindow = new ColorSelector();
            colorSelectorWindow.ViewModel.ColorSelected += SelectedColorChanged; // Subskrybuj zdarzenie
            colorSelectorWindow.Show();
        }

        private void SelectedColorChanged(System.Windows.Media.Color selectedColor)
        {
            // Zaktualizuj wartości kolorów w głównym oknie na podstawie wybranego koloru
            // Na przykład:
            // this.ViewModel.SelectedColor = selectedColor;
        }


    }
}
