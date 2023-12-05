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
using WPF_Paint.Models;
using WPF_Paint.ViewModels;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class BinarizationPercentView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _finalThreshold;
        private int[] _percentDistribution;

        public BinarizationPercentView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;
            _percentDistribution = CalculatePercentdistribution();
            InitializeComponent();

            _finalThreshold = CalculateThreshold();
            thresholdSlider.Value = _finalThreshold;
            _binarizationHelper.UpdateImageWithThreshold(_finalThreshold);

            
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

            _finalThreshold = CalculateThreshold();
            _binarizationHelper.UpdateImageWithThreshold(_finalThreshold);
        }

        private byte CalculateThreshold()
        {
            int sliderValue = (int)thresholdSlider.Value;

            int threshold = 255;

            for (int i = 0; i < 256; i++)
            {
                if (_percentDistribution[i] > sliderValue)
                {
                    threshold = i;
                    break;
                }
            }

            return (byte)threshold;
        }

        public int[] CalculatePercentdistribution()
        {
            int[] cdf = new int[256];
            cdf[0] = _binarizationHelper.Histogram[0];
            for (int i = 1; i < 256; i++)
            {
                cdf[i] = cdf[i - 1] + _binarizationHelper.Histogram[i];
            }

            // Normalize CDF
            for (int i = 0; i < cdf.Length; i++)
            {
                cdf[i] = (int)(((cdf[i]) / (float)(_binarizationHelper.PixelAmount)) * 100);
            }

            return cdf;
        }
    }
}
