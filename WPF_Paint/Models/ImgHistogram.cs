using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

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
        private int[] _cdf = new int[256];

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
        public int[] CDF
        {
            get { return _cdf; }
        }

        private byte[] _sourcePixels;
        private byte[] _equalPixels;

        public ImgHistogram(BitmapSource currentImage) 
        {
            int width = currentImage.PixelWidth;
            int height = currentImage.PixelHeight;

            // Konwersja obrazu na format array pikseli
            int stride = width * 4; // 4 kanały (RGBA) na piksel
            _sourcePixels = new byte[height * stride];
            _equalPixels = new byte[height * stride];
            currentImage.CopyPixels(_sourcePixels, stride, 0);
            currentImage.CopyPixels(_equalPixels, stride, 0);

            //For each pixel
            for (int i = 0; i < height; i++)
                for(int j = 0; j < width; j++)
                {
                    int pixelIndex = i * stride + j*4;
                    _blueHistogram[_sourcePixels[pixelIndex]]++;
                    _greenHistogram[_sourcePixels[pixelIndex+1]]++;
                    _redHistogram[_sourcePixels[pixelIndex+2]]++;
                    int grayscale = (int)(0.299 * _sourcePixels[pixelIndex] + 0.587 * _sourcePixels[pixelIndex + 1] + 0.114 * _sourcePixels[pixelIndex + 2]);
                    _histogram[grayscale]++;
                }

            EqualizeHistogram();
        }

        public void EqualizeHistogram()
        {
            int numPixels = _sourcePixels.Length / 4; 
            int[] cdfMinHistogram = _histogram.Where(h => h != 0).ToArray();
            int cdfMin = cdfMinHistogram.Length > 0 ? cdfMinHistogram.Min() : 0;

            // Calculate CDF
            _cdf = new int[256];
            _cdf[0] = _histogram[0];
            for (int i = 1; i < _histogram.Length; i++)
            {
                _cdf[i] = _cdf[i - 1] + _histogram[i];
            }

            // Normalize CDF
            for (int i = 0; i < _cdf.Length; i++)
            {
                _cdf[i] = (int)(((_cdf[i] - cdfMin) / (float)(numPixels - cdfMin)) * 255);
            }

            // Apply the equalized histogram to the image
            for (int i = 0; i < _sourcePixels.Length; i += 4)
            {
                _equalPixels[i] = (byte)_cdf[_sourcePixels[i]]; // Blue
                _equalPixels[i + 1] = (byte)_cdf[_sourcePixels[i + 1]]; // Green
                _equalPixels[i + 2] = (byte)_cdf[_sourcePixels[i + 2]]; // Red
                _equalPixels[i + 3] = _sourcePixels[i + 3]; // Alpha
                int grayscale = (int)(0.299 * _equalPixels[i] + 0.587 * _equalPixels[i + 1] + 0.114 * _equalPixels[i + 2]);
                _eqHistogram[grayscale]++;
            }
        }

    }
}
