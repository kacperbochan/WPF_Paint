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
using WPF_Paint.Models;
using WPF_Paint.ViewModels;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class BinarizationValueView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _finalThreshold;

        public BinarizationValueView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;
            _finalThreshold = 150;
            _binarizationHelper.UpdateImageWithThreshold(_finalThreshold);
            InitializeComponent();            
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == _finalThreshold) return;

            _finalThreshold = (byte)thresholdSlider.Value;
            _binarizationHelper.UpdateImageWithThreshold(_finalThreshold);
        }
    }
}
