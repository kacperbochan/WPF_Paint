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
using WPF_Paint.Models;
using static WPF_Paint.Models.HelperMethods;

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

            MainCanvas.MouseWheel += MainCanvas_MouseWheel;
        }

        private ViewModelColors ViewModel
        {
            get { return DataContext as ViewModelColors; }
        }


        private void OpenFillingColorSelector()
        {

            ColorSelector colorSelectorWindow = new ColorSelector();
            colorSelectorWindow.ViewModel.ColorSelected += SelectedColorChanged; // Subskrybuj zdarzenie
            colorSelectorWindow.Show();
        }

        private void OpenBorderColorSelector()
        {
            ColorSelector colorSelectorWindow = new ColorSelector();
            colorSelectorWindow.ViewModel.ColorSelected += SelectedColorChanged; // Subskrybuj zdarzenie
            colorSelectorWindow.Show();
        }

        private void MainCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var st = (ScaleTransform)MainCanvas.LayoutTransform;

            // Current scale factor
            double currentScale = st.ScaleX;

            // Determine the zoom direction and calculate zoom factor
            bool zoomingIn = e.Delta > 0;
            double zoomFactor = zoomingIn ? 0.1 : -0.1;

            // Apply a logarithmic approach to reduce zoom change as scale decreases
            if (!zoomingIn && currentScale < 1)
            {
                zoomFactor *= currentScale; // Reduce the zoom out effect as the scale gets smaller
            }

            // Calculate new scale
            double newScale = currentScale + zoomFactor;

            // Ensure new scale is positive and within bounds
            newScale = Math.Max(newScale, 0.1); // Prevent it from going below a certain threshold (e.g., 0.1)

            // Apply the new scale
            st.ScaleX = st.ScaleY = newScale;
        }


        private void SelectedColorChanged(System.Windows.Media.Color selectedColor)
        {
            // Zaktualizuj wartości kolorów w głównym oknie na podstawie wybranego koloru
            // Na przykład:
            // this.ViewModel.SelectedColor = selectedColor;
        }


    }
}
