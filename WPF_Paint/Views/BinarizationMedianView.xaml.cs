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
    public partial class BinarizationMedianView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _finalThreshold;

        public BinarizationMedianView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;
            _finalThreshold = CalculateFinalThreshold();

            _binarizationHelper.UpdateImageWithThreshold(_finalThreshold);

            InitializeComponent();

            thresholdSlider.Value = _finalThreshold;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }


        private byte ComputeNewThreshold(byte oldThreshold) 
        {
            long sumBackground = 0, sumForeground = 0;
            long countBackground = 0, countForeground = 0;

            for (int i = oldThreshold+1; i < 256; i++)
            {
                sumBackground += i * _binarizationHelper.Histogram[i];
                countBackground += _binarizationHelper.Histogram[i];
            }
            sumForeground = _binarizationHelper.PixelSum - sumBackground;
            countForeground = _binarizationHelper.PixelAmount - countBackground;

            byte meanBackground = (byte)(sumBackground / Math.Max(1, countBackground));
            byte meanForeground = (byte)(sumForeground / Math.Max(1, countForeground));

            return (byte)((meanBackground + meanForeground) / 2);
        }

        private byte CalculateFinalThreshold()
        {
            byte threshold = (byte)(_binarizationHelper.PixelSum / _binarizationHelper.PixelAmount);

            bool thresholdChanged;
            do
            {
                thresholdChanged = false;
                byte newThreshold = ComputeNewThreshold(threshold);
                if (newThreshold != threshold)
                {
                    threshold = newThreshold;
                    thresholdChanged = true;
                }
            }
            while (thresholdChanged);

            return threshold;
        }
    }
}
