using System;
using System.Windows;
using WPF_Paint.Models;

namespace WPF_Paint
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class BinarizationPansalkarView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _radius = 0;
        private byte[] _bitmapBuffer;
        private double[] _meanBuffer, _stddevBuffer;
        private double _k = 0.25;
        private double _q = 10;
        private double _p = 2;
        private int R = 128;

        public BinarizationPansalkarView(BinarizationHelper binarizationHelper)
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
            if (Math.Round(e.NewValue,2) == _k) return;

            _k = Math.Round(e.NewValue,2);

            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void RSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((byte)Math.Round(e.NewValue) == R) return;

            R = (byte)Math.Round(e.NewValue,0);

            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void QSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((byte)Math.Round(e.NewValue, 0) == _q) return;

            _q = (byte)Math.Round(e.NewValue, 0);

            CalculateBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void PSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((byte)Math.Round(e.NewValue,1) == _p) return;

            _p = (byte)Math.Round(e.NewValue, 1);

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
            int threshold;
            for (int i = 0; i < _bitmapBuffer.Length; i++) {
                threshold = (byte)(
                    _meanBuffer[i] *
                    (1
                    +
                    _p * Math.Pow(
                        Math.E,
                        -1 * _q * _meanBuffer[i])
                    +
                    _k * (_stddevBuffer[i] / R - 1))
                    );
                _bitmapBuffer[i] = (byte)(_binarizationHelper.GrayScale[i] > threshold ? 255 : 0);
            }
        }

    }
}
