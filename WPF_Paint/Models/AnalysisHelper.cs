﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WPF_Paint.Models
{
    public class AnalysisHelper
    {
        private Canvas _canvas;
        private WriteableBitmap _writableBitmap;
        private byte[] _originalPixels;
        private byte[] _bufferPixels;
        private int _domValue = 20;

        private byte[] _redDomBin;
        private byte[] _greenDomBin;
        private byte[] _blueDomBin;
        private double _redPercent; 
        private double _greenPercent; 
        private double _bluePercent; 

        public double RedPercent
        {
            get { return _redPercent; }
        }
        public double GreenPercent
        {
            get { return _greenPercent; }
        }
        public double BluePercent
        {
            get { return _bluePercent; }
        }


        private int _width = 0;
        private int _height = 0;
        private int _stride = 0;

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }

        public AnalysisHelper(BitmapSource source, Canvas canvas)
        {
            _canvas = canvas;
            _width = source.PixelWidth;
            _height = source.PixelHeight;

            _writableBitmap = new WriteableBitmap(source);

            _stride = Width * 4;

            _originalPixels = new byte[ Height * _stride];
            _bufferPixels = new byte[Height * _stride];
            _writableBitmap.CopyPixels(_originalPixels, _stride, 0);
            _writableBitmap.CopyPixels(_bufferPixels, _stride, 0);
        }

        private void GetColorBitmaps()
        {
            _redDomBin = new byte[Width * Height];
            _greenDomBin = new byte[Width * Height];
            _blueDomBin = new byte[Width * Height];
            _redPercent = 0;
            _greenPercent = 0;
            _bluePercent = 0;

            int red, green, blue;
            int bitmapIndex, index=0;
            for (int y = 0; y < Height; y++) 
            {
                for(int x = 0; x < Width; x++)
                {
                    bitmapIndex = ((y * Width) + x) * 4;
                    
                    red = _originalPixels[bitmapIndex + 2];
                    green = _originalPixels[bitmapIndex + 1];
                    blue = _originalPixels[bitmapIndex];

                    // Unzajemy, że kolor dominuje jeśli jest od pozostałych dwóch conajmniej wyższy o 50
                    // Tak, jest opcja, że żaden z nich nie dominuje
                    if(red >= (green + _domValue) && red >= (blue + _domValue))
                    {
                        _redDomBin[index] = 1;
                        _greenDomBin[index] = 0;
                        _blueDomBin[index] = 0;
                        _redPercent += 1;
                    }
                    else if (green >= (red + _domValue) && green >= (blue + _domValue))
                    {
                        _redDomBin[index] = 0;
                        _greenDomBin[index] = 1;
                        _blueDomBin[index] = 0;
                        _greenPercent += 1;
                    }
                    else if(blue >= (red + _domValue) && blue >= (green + _domValue))
                    {
                        _redDomBin[index] = 0;
                        _greenDomBin[index] = 0;
                        _blueDomBin[index] = 1;
                        _bluePercent += 1;
                    }
                    
                    index++;
                }
            }

            _redPercent /= Width * Height;
            _greenPercent /= Width * Height;
            _bluePercent /= Width * Height;
        }

        public void SetBufferColorDom(int color, int domValue=-1)
        {
            if (domValue != -1 && domValue != _domValue)
                _domValue = domValue;
                GetColorBitmaps();

            if (_blueDomBin == null) GetColorBitmaps();

            int binLength = _redDomBin.Length;
            byte[] _colorDomBin;

            switch (color)
            {
                case 0:
                    _colorDomBin = _redDomBin;
                    break;
                case 1:
                    _colorDomBin = _greenDomBin;
                    break;
                default:
                    _colorDomBin = _blueDomBin;
                    break;
            }

            for (int i = 0; i < binLength; i++)
            {
                int bitmapId = i * 4;
                if (_colorDomBin[i] == 1)
                {
                    _bufferPixels[bitmapId] = _originalPixels[bitmapId];
                    _bufferPixels[bitmapId + 1] = _originalPixels[bitmapId + 1];
                    _bufferPixels[bitmapId + 2] = _originalPixels[bitmapId + 2];
                }
                else
                {
                    _bufferPixels[bitmapId] = 255;
                    _bufferPixels[bitmapId + 1] = 255;
                    _bufferPixels[bitmapId + 2] = 255;
                }
            }
        }

        public void ReplaceImage()
        {
            _writableBitmap.WritePixels(new Int32Rect(0, 0, _width, _height), _bufferPixels, _stride, 0);

            Image replacementImage = new Image();
            replacementImage.Source = _writableBitmap;

            _canvas.Children.Clear();
            _canvas.Children.Add(replacementImage);

        }
    }
}
