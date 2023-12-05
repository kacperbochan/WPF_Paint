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
    public partial class BinarizationOtsuView : Window
    {
        private BinarizationHelper _binarizationHelper;
        byte threshold = 0;

        public BinarizationOtsuView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;

            threshold = CalculateTreshold();

            _binarizationHelper.UpdateImageWithThreshold(threshold);

            InitializeComponent();

            thresholdSlider.Value = threshold;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private byte CalculateTreshold() 
        {
            double sumB = 0;
            long wB = 0, wF = 0;
            double maxVar = 0.0;
            int threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += _binarizationHelper.Histogram[i];            // Weight Background
                if (wB == 0) continue;

                wF = _binarizationHelper.PixelAmount - wB;                   // Weight Foreground
                if (wF == 0) break;

                sumB += (double)(i * _binarizationHelper.Histogram[i]);

                double mB = sumB / wB;             // Mean Background
                double mF = (_binarizationHelper.PixelSum - sumB) / wF;     // Mean Foreground

                // Calculate Between Class Variance
                double varBetween = (double)wB * (double)wF * (mB - mF) * (mB - mF);

                // Check if new maximum found
                if (varBetween > maxVar)
                {
                    maxVar = varBetween;
                    threshold = i;
                }
            }
            return (byte)threshold;
        }
        
    }
}
