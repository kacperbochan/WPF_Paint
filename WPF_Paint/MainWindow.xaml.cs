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
    }
}
