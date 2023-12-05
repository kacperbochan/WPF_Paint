using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Ink;
using System.Windows.Controls;

namespace WPF_Paint.Models
{
    public class ImgHistogram
    {
        private BitmapSource currentImage;

        private int[] _histogram = new int[256];
        private int[] _eqHistogram = new int[256];
        private int[] _redHistogram = new int[256];
        private int[] _greenHistogram = new int[256];
        private int[] _blueHistogram = new int[256];
        private int[] _eqRedHistogram = new int[256];
        private int[] _eqGreenHistogram = new int[256];
        private int[] _eqBlueHistogram = new int[256];
        private int[] _cdf = new int[256];
        private int[] _cdfRed = new int[256];
        private int[] _cdfGreen = new int[256];
        private int[] _cdfBlue = new int[256];
        private int[] _cdfEq = new int[256];
        private int[] _cdfEqRed = new int[256];
        private int[] _cdfEqGreen = new int[256];
        private int[] _cdfEqBlue = new int[256];

        public int[] Histogram
        {
            get { return _histogram; }
        }
        public int[] EqHistogram
        {
            get { return _eqHistogram; }
        }
        public int[] RedHistogram
        {
            get { return _redHistogram; }
        }
        public int[] GreenHistogram
        {
            get { return _greenHistogram; }
        }
        public int[] BlueHistogram
        {
            get { return _blueHistogram; }
        }
        public int[] EqRedHistogram
        {
            get { return _eqRedHistogram; }
        }
        public int[] EqGreenHistogram
        {
            get { return _eqGreenHistogram; }
        }
        public int[] EqBlueHistogram
        {
            get { return _eqBlueHistogram; }
        }

        public int[] Cdf
        {
            get { return _cdf; }
        }

        public int[] CdfRed
        {
            get { return _cdfRed; }
        }
        public int[] CdfGreen
        {
            get { return _cdfGreen; }
        }
        public int[] CdfBlue
        {
            get { return _cdfBlue; }
        }
        public int[] CdfEq
        {
            get { return _cdfEq; }
        }
        public int[] CdfEqRed
        {
            get { return _cdfEqRed; }
        }
        public int[] CdfEqGreen
        {
            get { return _cdfEqGreen; }
        }
        public int[] CdfEqBlue
        {
            get { return _cdfEqBlue; }
        }

        private byte[] _sourcePixels;
        private byte[] _equalPixels;
        public byte[] EqualizedPixels
        {
            get { return _equalPixels; }
        }

        private int width;
        private int height;

        public ImgHistogram(BitmapSource currentImage) 
        {
            width = currentImage.PixelWidth;
            height = currentImage.PixelHeight;

            int stride = width * 4;
            _sourcePixels = new byte[height * stride];
            _equalPixels = new byte[height * stride];
            currentImage.CopyPixels(_sourcePixels, stride, 0);
            currentImage.CopyPixels(_equalPixels, stride, 0);

            CalculateHistogram();
            EqualizeHistograms();
            CalculateEqHistogram();
        }

        private void CalculateHistogram()
        {
            int stride = width * 4;
            
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int pixelIndex = i * stride + j * 4;
                    _blueHistogram[_sourcePixels[pixelIndex]]++;
                    _greenHistogram[_sourcePixels[pixelIndex + 1]]++;
                    _redHistogram[_sourcePixels[pixelIndex + 2]]++;
                    int grayscale = (int)(_sourcePixels[pixelIndex] + _sourcePixels[pixelIndex + 1] + _sourcePixels[pixelIndex + 2]) / 3;
                    _histogram[grayscale]++;
                }
        }

        private void CalculateEqHistogram()
        {
            int stride = width * 4;

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    int pixelIndex = i * stride + j * 4;
                    _eqBlueHistogram[_equalPixels[pixelIndex]]++;
                    _eqGreenHistogram[_equalPixels[pixelIndex + 1]]++;
                    _eqRedHistogram[_equalPixels[pixelIndex + 2]]++;
                    int grayscale = (int)(_equalPixels[pixelIndex] + _equalPixels[pixelIndex + 1] + _equalPixels[pixelIndex + 2]) / 3;
                    _eqHistogram[grayscale]++;
                }

            _cdfEq = CalculateCDF(_eqHistogram);
            _cdfEqRed = CalculateCDF(_eqRedHistogram);
            _cdfEqGreen = CalculateCDF(_eqGreenHistogram);
            _cdfEqBlue = CalculateCDF(_eqBlueHistogram);
        }

        private int[] CalculateCDF(int[] histogram)
        {
            long numPixels = width * height; // Total number of pixels for the channel
            int cdfMin = histogram.First(h => h != 0); // Minimum non-zero value in the histogram

            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < histogram.Length; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // Normalize CDF
            for (int i = 0; i < cdf.Length; i++)
            {
                cdf[i] = (int)(((cdf[i] - cdfMin) / (float)(numPixels - cdfMin)) * 255);
            }

            return cdf;
        }
        
        public int[] CalculatePercentdistribution(int[] histogram)
        {
            long numPixels = width*height; // Total number of pixels for the channel

            int[] cdf = new int[256];
            cdf[0] = histogram[0];
            for (int i = 1; i < histogram.Length; i++)
            {
                cdf[i] = cdf[i - 1] + histogram[i];
            }

            // Normalize CDF
            for (int i = 0; i < cdf.Length; i++)
            {
                cdf[i] = (int)(((cdf[i]) / (float)(numPixels)) * 100);
            }

            return cdf;
        }

        public void EqualizeHistograms()
        {
            // Calculate CDF for each channel
            _cdf = CalculateCDF(_histogram);
            _cdfRed = CalculateCDF(_redHistogram);
            _cdfGreen = CalculateCDF(_greenHistogram);
            _cdfBlue = CalculateCDF(_blueHistogram);


            // Apply the equalized histogram to each channel of the image
            for (int i = 0; i < _sourcePixels.Length; i += 4)
            {
                _equalPixels[i] = (byte)_cdfBlue[_sourcePixels[i]]; // Blue
                _equalPixels[i + 1] = (byte)_cdfGreen[_sourcePixels[i + 1]]; // Green
                _equalPixels[i + 2] = (byte)_cdfRed[_sourcePixels[i + 2]]; // Red
                _equalPixels[i + 3] = _sourcePixels[i + 3]; // Alpha (no change)
            }
        }

        public BitmapSource EqualBitmapSource()
        {
            int stride = width * 4; // 4 bytes per pixel in RGBA format

            WriteableBitmap writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            writeableBitmap.WritePixels(new Int32Rect(0, 0, width, height), _equalPixels, stride, 0);

            return writeableBitmap;
        }

    }
}
