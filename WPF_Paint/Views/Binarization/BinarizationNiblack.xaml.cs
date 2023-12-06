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
    public partial class BinarizationNiblackView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _radius = 0;
        private byte[] _bitmapBuffer;
        private double[] _meanBuffer, _stddevBuffer;
        private double _k = -0.2;

        public BinarizationNiblackView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;
            _bitmapBuffer = new byte[_binarizationHelper.GrayScale.Length];
            _meanBuffer = new double[_binarizationHelper.GrayScale.Length];
            _stddevBuffer = new double[_binarizationHelper.GrayScale.Length];

            InitializeComponent();

            CalculateMeanAndStddev();
            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((byte)radiusSlider.Value == _radius) return;

            _radius = (byte)radiusSlider.Value;

            CalculateMeanAndStddev();
            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }
        

        private void KSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue) == _k) return;

            _k = Math.Round(e.NewValue,1);

            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private (double,double) PixelFilter(int x, int y) 
        {
            byte[] buffer = new byte[(2 * _radius + 1) * (2 * _radius + 1)];

            int sum = 0;
            int id = 0;

            for(int i = -_radius; i <= _radius; i++)
            {
                int offsetY = Math.Clamp(y + i, 0, _binarizationHelper.Height-1) * _binarizationHelper.Width;
                for (int j = -_radius; j <= _radius; j++)
                {
                    int offset = offsetY + Math.Clamp(x + j, 0, _binarizationHelper.Width-1);

                    byte pixelValue = _binarizationHelper.GrayScale[offset];

                    buffer[id++] = pixelValue;
                    sum += pixelValue;
                }
            }

            double mean = sum/buffer.Length;

            double variance = 0;
            for(int i=0; i<buffer.Length; i++)
                variance += (buffer[i] - mean) * (buffer[i] - mean);
            variance /= buffer.Length;
            double stddev = Math.Sqrt(variance);

            return (mean, stddev);
        }

        private void CalculateMeanAndStddev()
        {
            for (int y = 0; y < _binarizationHelper.Height; y++) 
            {
                for (int x = 0; x < _binarizationHelper.Width; x++)
                {
                    int id = y * _binarizationHelper.Width + x;

                    double mean, stddev;
                    (mean, stddev) = PixelFilter(x, y);

                    _meanBuffer[id] = mean;
                    _stddevBuffer[id] = stddev;
                }
            }
        }

        private void CalculateBitmap()
        {
            for (int i = 0; i < _bitmapBuffer.Length; i++)
                _bitmapBuffer[i] = (byte)((_binarizationHelper.GrayScale[i]>(byte)(_meanBuffer[i] + _k * _stddevBuffer[i]))?255:0);
        }
        
    }
}
