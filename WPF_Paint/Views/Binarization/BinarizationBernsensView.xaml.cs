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
    public partial class BinarizationBernsensView : Window
    {
        private BinarizationHelper _binarizationHelper;
        private byte _radius = 0;
        private byte[] _bitmapBuffer;

        public BinarizationBernsensView(BinarizationHelper binarizationHelper)
        {
            _binarizationHelper = binarizationHelper;

            InitializeComponent();

            CalculateNewBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void RadiusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Round(e.NewValue) == _radius) return;

            _radius = (byte)Math.Round(e.NewValue);

            CalculateNewBitmap();
            _binarizationHelper.UpdateImageWithByteMap(_bitmapBuffer);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            // Zamknij okno
            this.Close();
        }

        private byte[] CloneGrayscale()
        {
            byte[] bitmapBuffer = new byte[_binarizationHelper.GrayScale.Length];
            for(int i= 0; i<bitmapBuffer.Length; i++)
            {
                bitmapBuffer[i] = _binarizationHelper.GrayScale[i];
            }
            return bitmapBuffer;
        }

        private byte PixelFilter(int x, int y) 
        {
            byte min=255, max=0;

            for(int i = -_radius; i <= _radius; i++)
            {
                int offsetY = Math.Clamp(y + i, 0, _binarizationHelper.Height-1) * _binarizationHelper.Width;
                for (int j = -_radius; j <= _radius; j++)
                {
                    int offset = offsetY + Math.Clamp(x + j, 0, _binarizationHelper.Width-1);

                    byte pixelValue = _binarizationHelper.GrayScale[offset];

                    if (pixelValue > max) 
                    {
                        max = pixelValue;
                    }
                    if (pixelValue < min)
                    {
                        min = pixelValue;
                    }
                }
            }

            int pixelId = y * _binarizationHelper.Width + x;
            byte threshold = (byte)(((int)max + (int)min) / 2);

            return (byte)((_binarizationHelper.GrayScale[pixelId]>threshold)?0:255);
        }

        private void CalculateNewBitmap()
        {
            _bitmapBuffer = CloneGrayscale();

            for (int y = 0; y < _binarizationHelper.Height; y++) 
            {
                for (int x = 0; x < _binarizationHelper.Width; x++)
                {
                    _bitmapBuffer[y * _binarizationHelper.Width + x] = PixelFilter(x, y);
                }
            }
        }
        
    }
}
