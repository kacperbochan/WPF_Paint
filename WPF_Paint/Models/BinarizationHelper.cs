using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPF_Paint.Models
{
    public class BinarizationHelper
    {
        private Canvas _canvas;
        private WriteableBitmap _writableBitmap;
        private byte[] _originalGrayScale;
        private byte[] _bufforImage;

        private int[] valueMapping = new int[256];

        private int[] _histogram;

        private int _width = 0;
        private int _height = 0;
        private long _pixelSum = 0;
        private long _pixelAmount = 0;

        public byte[] GrayScale { get { return _originalGrayScale; } }
        public int[] Histogram { get { return _histogram; } }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public long PixelSum { get { return _pixelSum; } }
        public long PixelAmount { get { return _pixelAmount; } }

        private byte Threshold = 0;

        public BinarizationHelper(BitmapSource source, Canvas canvas)
        {
            _histogram = new ImgHistogram(source).Histogram;
            _width = source.PixelWidth;
            _height = source.PixelHeight;
            _bufforImage = new byte[_width * _height];

            _pixelAmount = _width * _height; // Total number of pixels for the channel

            for (int i = 0; i < 256; i++)
                _pixelSum += i * _histogram[i];

            GetGrayScale(source);
            _writableBitmap = new WriteableBitmap(source);

            _canvas = canvas;
        }

        private void GetGrayScale(BitmapSource source)
        {
            int stride = _width * 4;

            byte[] sourcePixels = new byte[_height * stride];
            source.CopyPixels(sourcePixels, stride, 0);
            _originalGrayScale = new byte[_width * _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int pixelIndex = y * stride + x * 4;
                    int grayscale = (sourcePixels[pixelIndex] + sourcePixels[pixelIndex + 1] + sourcePixels[pixelIndex + 2]) / 3;
                    _originalGrayScale[y * _width + x] = (byte)grayscale;
                }
            }
        }

        public void UpdateImageWithThreshold(byte threshold)
        {
            Threshold = threshold;
            GetValueMapping();
            MapTheValue();
            ReplaceImage();
        }

        private void GetValueMapping()
        {
            for (int i = 0; i < Threshold; i++)
            {
                valueMapping[i] = 0;
            }
            for (int i = Threshold; i < 256; i++)
            {
                valueMapping[i] = 255;
            }
        }

        private void MapTheValue()
        {
            for (int i = 0; i < _originalGrayScale.Length; i++)
            {
                _bufforImage[i] = (byte)valueMapping[_originalGrayScale[i]];
            }
        }

        private void ReplaceImage()
        {
            int stride = _width * 4; // 4 bytes per pixel in RGBA format

            byte[] sourcePixels = new byte[_height * stride];
            _writableBitmap.CopyPixels(sourcePixels, stride, 0);
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = y * stride + x * 4;
                    int bufforindex = y * _width + x;

                    sourcePixels[index] = (byte)_bufforImage[bufforindex];
                    sourcePixels[index + 1] = (byte)_bufforImage[bufforindex];
                    sourcePixels[index + 2] = (byte)_bufforImage[bufforindex];
                }
            }

            _writableBitmap.WritePixels(new Int32Rect(0, 0, _width, _height), sourcePixels, stride, 0);

            Image equalImage = new Image();
            equalImage.Source = _writableBitmap;

            _canvas.Children.Clear();
            _canvas.Children.Add(equalImage);

        }

    }
}
